using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class DependencyGroup
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "targetFramework")]
        public string TargetFramework { get; set; }

        [JsonProperty(PropertyName = "dependencies")]
        public List<Dependency> Dependencies { get; set; }
    }
}
