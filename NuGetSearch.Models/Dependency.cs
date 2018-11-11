using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class Dependency
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string PackageName { get; set; }

        [JsonProperty(PropertyName = "range")]
        public string Range { get; set; }
    }
}