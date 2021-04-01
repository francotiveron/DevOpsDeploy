using ReleaseRetention.Library;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;


namespace ReleaseRetention
{
    class Program
    {
        static void Main(string[] args)
        {
            var projects = JsonSerializer.Deserialize<HashSet<Project>>(File.ReadAllText(@"..\..\..\Json\Projects.json"));
            var releases = JsonSerializer.Deserialize<HashSet<Release>>(File.ReadAllText(@"..\..\..\Json\Releases.json"));
            var environments = JsonSerializer.Deserialize<HashSet<Environ>>(File.ReadAllText(@"..\..\..\Json\Environments.json"));
            var deployments = JsonSerializer.Deserialize<HashSet<Deployment>>(File.ReadAllText(@"..\..\..\Json\Deployments.json"));
            var keep = new ReleaseManager(projects!, releases!, environments!, deployments!, s => Console.WriteLine(s)).Keep(5);
        }
    }
}
