using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;


namespace ReleaseRetention
{
    public struct Deployment
    {
        public string Id { get; set; }
        public string ReleaseId { get; set; }
        public string EnvironmentId { get; set; }
        public DateTime DeployedAt { get; set; }
        public override string ToString() => $"Id = {Id}, ReleaseId = {ReleaseId}, EnvironmentId = {EnvironmentId}, DeployedAt = {DeployedAt}";
    }

    public struct Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public struct Release
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Version { get; set; }
        public DateTime Created { get; set; }
        public override string ToString() => $"Id = {Id}, ProjectId = {ProjectId}, Version = {Version}, Created = {Created}";
    }

    public struct Environment
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var projects = JsonSerializer.Deserialize<HashSet<Project>>(File.ReadAllText(@"..\..\..\Json\Projects.json"));
            var releases = JsonSerializer.Deserialize<HashSet<Release>>(File.ReadAllText(@"..\..\..\Json\Releases.json"));
            var orphanReleases = releases.Where(r => projects.All(p => p.Id != r.ProjectId)).ToList();
            if (orphanReleases.Count > 0)
            {
                Console.WriteLine("The following release(s) do(es) not correspond to a known project and will be ignored:");
                foreach (var r in orphanReleases) Console.WriteLine(r);
                releases.ExceptWith(orphanReleases);
            }
            var environments = JsonSerializer.Deserialize<HashSet<Environment>>(File.ReadAllText(@"..\..\..\Json\Environments.json"));
            var deployments = JsonSerializer.Deserialize<HashSet<Deployment>>(File.ReadAllText(@"..\..\..\Json\Deployments.json"));
            var orphanDeployments = deployments.Where(d => environments.All(e => e.Id != d.EnvironmentId)).ToList();
            if (orphanDeployments.Count > 0)
            {
                Console.WriteLine("The following deployment(s) do(es) not correspond to a known environment and will be ignored:");
                foreach (var d in orphanDeployments) Console.WriteLine(d);
                deployments.ExceptWith(orphanDeployments);
            }
            orphanDeployments = deployments.Where(d => releases.All(r => r.Id != d.ReleaseId)).ToList();
            if (orphanDeployments.Count > 0)
            {
                Console.WriteLine("The following deployment(s) do(es) not correspond to a known release and will be ignored:");
                foreach (var d in orphanDeployments) Console.WriteLine(d);
                deployments.ExceptWith(orphanDeployments);
            }

            IEnumerable<(string projId, string envId)> Combine(HashSet<Project> projects, HashSet<Environment> environments)
            {
                foreach (var project in projects) foreach (var environment in environments) yield return (project.Id, environment.Id);
            }
            var releasesByProEnv = new Dictionary<(string projId, string envId), List<Release>>();
            foreach (var proEnv in Combine(projects, environments)) releasesByProEnv[proEnv] = new List<Release>();
            var releasesDict = releases.ToDictionary(r => r.Id);
            foreach(var deployment in deployments.OrderByDescending(d => d.DeployedAt))
            {
                var release = releasesDict[deployment.ReleaseId];
                var envId = deployment.EnvironmentId;
                releasesByProEnv[(release.ProjectId, deployment.EnvironmentId)].Add(release);
            }
            var keep = new HashSet<Release>();
            var n = 4;
            foreach(var proEnvReleases in releasesByProEnv.Values)
            {
                foreach (var release in proEnvReleases.Take(n)) keep.Add(release);
            }
        }
    }
}
