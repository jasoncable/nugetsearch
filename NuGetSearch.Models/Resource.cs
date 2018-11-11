using System;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class Resource
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }
    }
}
