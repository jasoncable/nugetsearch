using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class Services
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "resources")]
        public List<Resource> Resources { get; set; }
    }
}
