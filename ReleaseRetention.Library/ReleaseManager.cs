using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseRetention.Library
{
    public class ReleaseManager
    {
        public const uint MaxReleases = 100;
        readonly HashSet<Project> _projects;
        readonly HashSet<Release> _releases;
        readonly HashSet<Environ> _environments;
        readonly HashSet<Deployment> _deployments;
        readonly Action<string> _logger;
        
        static T Check<T>(T arg, [CallerArgumentExpression("arg")] string? name = default(string)) where T : class
        {
            if (arg == null) throw new ArgumentNullException(name);
            return arg;
        }

        public ReleaseManager(HashSet<Project> projects, HashSet<Release> releases, HashSet<Environ> environments, HashSet<Deployment> deployments, Action<string>? logger = null)
        {
            _projects = Check(projects);
            _releases = Check(releases);
            _environments = Check(environments);
            _deployments = Check(deployments);
            _logger = logger ?? (_ => { });
        }

        public HashSet<Release> Keep(uint n)
        {
            if (n > MaxReleases) throw new ArgumentOutOfRangeException(nameof(n), n, $"n must not exceed {MaxReleases}");

            if (n == 0) return new HashSet<Release>();
            var orphanReleases = _releases.Where(r => _projects.All(p => p.Id != r.ProjectId)).ToList();
            if (orphanReleases.Count > 0)
            {
                _logger("The following release(s) do(es) not correspond to a known project and will be ignored:");
                foreach (var r in orphanReleases) Console.WriteLine(r);
                _releases.ExceptWith(orphanReleases);
            }
            var orphanDeployments = _deployments.Where(d => _environments.All(e => e.Id != d.EnvironmentId)).ToList();
            if (orphanDeployments.Count > 0)
            {
                _logger("The following deployment(s) do(es) not correspond to a known environment and will be ignored:");
                foreach (var d in orphanDeployments) Console.WriteLine(d);
                _deployments.ExceptWith(orphanDeployments);
            }
            orphanDeployments = _deployments.Where(d => _releases.All(r => r.Id != d.ReleaseId)).ToList();
            if (orphanDeployments.Count > 0)
            {
                _logger("The following deployment(s) do(es) not correspond to a known release and will be ignored:");
                foreach (var d in orphanDeployments) Console.WriteLine(d);
                _deployments.ExceptWith(orphanDeployments);
            }

            IEnumerable<(string projId, string envId)> Combine(HashSet<Project> projects, HashSet<Environ> environments)
            {
                foreach (var project in projects) foreach (var environment in environments) yield return (project.Id, environment.Id);
            }
            var releasesByProEnv = new Dictionary<(string projId, string envId), List<Release>>();
            foreach (var proEnv in Combine(_projects, _environments)) releasesByProEnv[proEnv] = new List<Release>();
            var releasesDict = _releases.ToDictionary(r => r.Id);
            foreach (var deployment in _deployments.OrderByDescending(d => d.DeployedAt))
            {
                var release = releasesDict[deployment.ReleaseId];
                var envId = deployment.EnvironmentId;
                releasesByProEnv[(release.ProjectId, deployment.EnvironmentId)].Add(release);
            }

            var projectsDict = _projects.ToDictionary(p => p.Id);
            var environmentsDict = _environments.ToDictionary(e => e.Id);
            List<(Release release, string envId)> kept = new();

            foreach (var kvp in releasesByProEnv)
            {
                (_, var envId) = kvp.Key;
                var proEnvReleases = kvp.Value;
                foreach (var release in proEnvReleases.Take((int)n))
                {
                    kept.Add((release, envId));
                }
            }

            foreach (var group in kept.GroupBy(k => k.release))
            {
                var release = group.Key;
                var eIds = group.Select(k => k.envId).Distinct();
                _logger($"Release for version {release.Version} of project {projectsDict[release.ProjectId].Name} is kept as it was used in the latest {n} deployment(s) to environment(s) [{string.Join(", ", eIds.Select(eId => environmentsDict[eId].Name))}]");
            }
            return kept.Select(k => k.release).Distinct().ToHashSet();
        }
    }
}
