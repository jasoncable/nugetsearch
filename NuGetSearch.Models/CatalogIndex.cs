using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NuGetSearch.Models
{
    public class CatalogIndex
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public List<string> Types { get; set; }


        [JsonProperty(PropertyName = "commitId")]
        public Guid CommitId { get; set; }

        [JsonProperty(PropertyName = "commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "nuget:lastCreated")]
        public DateTime LastCreated { get; set; }
        [JsonProperty(PropertyName = "nuget:lastDeleted")]
        public DateTime LastDeleted { get; set; }
        [JsonProperty(PropertyName = "nuget:lastEdited")]
        public DateTime LastEdited { get; set; }

        [JsonProperty(PropertyName = "items")]
        public List<CatalogPage> Items { get; set; }

    }
}
