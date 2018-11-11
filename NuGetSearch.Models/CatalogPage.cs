using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class CatalogPage
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "commitId")]
        public Guid CommitId { get; set; }

        [JsonProperty(PropertyName = "commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<CatalogItem> Items { get; set; } = new List<CatalogItem>();

        [JsonIgnore]
        public bool IsDeleted { get { return this.Type == "nuget:PackageDelete";  } }
    }
}
