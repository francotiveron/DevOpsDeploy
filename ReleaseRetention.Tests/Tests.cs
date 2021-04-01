using ReleaseRetention.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace ReleaseRetention.Tests
{
    public class Tests : IClassFixture<SampleDataFixture>
    {
        SampleDataFixture _f;
        public Tests(SampleDataFixture fixture)
        {
            _f = fixture;
        }

        [Theory]
        [InlineData(1, new[] {1, 2, 6})]
        [InlineData(2, new[] {1, 2, 6, 7})]
        [InlineData(3, new[] {1, 2, 6, 7})]
        [InlineData(4, new[] {1, 2, 5, 6, 7})]
        [InlineData(5, new[] {1, 2, 5, 6, 7})]
        [InlineData(10, new[] {1, 2, 5, 6, 7})]
        public void sample_data_with_various_n(uint n, int[] rels)
        {
            Assert.True(_f.Manager.Keep(n).Select(rel => rel.Id).ToHashSet().SetEquals(rels.Select(rel => $"Release-{rel}").ToHashSet()));
        }

        [Fact]
        public void n_is_0()
        {
            Assert.Empty(_f.Manager.Keep(0));
        }

        [Fact]
        public void n_too_large()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _f.Manager.Keep(ReleaseManager.MaxReleases + 1));

        }

        [Fact]
        public void null_inputs()
        {
            Assert.Throws<ArgumentNullException>(() => new ReleaseManager(null!, _f.Releases, _f.Environments, _f.Deployments));
            Assert.Throws<ArgumentNullException>(() => new ReleaseManager(_f.Projects, null!, _f.Environments, _f.Deployments));
            Assert.Throws<ArgumentNullException>(() => new ReleaseManager(_f.Projects, _f.Releases, null!, _f.Deployments));
            Assert.Throws<ArgumentNullException>(() => new ReleaseManager(_f.Projects, _f.Releases, _f.Environments, null!));
        }

        [Fact]
        public void empty_inputs()
        {
            Assert.Empty(new ReleaseManager(new HashSet<Project>(), _f.Releases, _f.Environments, _f.Deployments).Keep(1));
            Assert.Empty(new ReleaseManager(_f.Projects, new HashSet<Release>(), _f.Environments, _f.Deployments).Keep(1));
            Assert.Empty(new ReleaseManager(_f.Projects, _f.Releases, new HashSet<Environ>(), _f.Deployments).Keep(1));
            Assert.Empty(new ReleaseManager(_f.Projects, _f.Releases, _f.Environments, new HashSet<Deployment>()).Keep(1));

            //Assert.Throws<ArgumentNullException>(() => new ReleaseManager(_f.Projects, _f.Releases, _f.Environments, _f.Deployments));
        }
    }

    public class SampleDataFixture
    {
        public SampleDataFixture()
        {
            Projects = JsonSerializer.Deserialize<HashSet<Project>>(File.ReadAllText(@"..\..\..\Json\Projects.json"))!;
            Releases = JsonSerializer.Deserialize<HashSet<Release>>(File.ReadAllText(@"..\..\..\Json\Releases.json"))!;
            Environments = JsonSerializer.Deserialize<HashSet<Environ>>(File.ReadAllText(@"..\..\..\Json\Environments.json"))!;
            Deployments = JsonSerializer.Deserialize<HashSet<Deployment>>(File.ReadAllText(@"..\..\..\Json\Deployments.json"))!;
            Manager = new ReleaseManager(Projects, Releases, Environments, Deployments);
        }
        public HashSet<Project> Projects { get; }
        public HashSet<Release> Releases { get; }
        public HashSet<Environ> Environments { get; }
        public HashSet<Deployment> Deployments { get; }
        public ReleaseManager Manager { get; }
    }
}
