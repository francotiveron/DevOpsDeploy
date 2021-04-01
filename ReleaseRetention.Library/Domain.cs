using System;

namespace ReleaseRetention.Library
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

    public struct Environ
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
