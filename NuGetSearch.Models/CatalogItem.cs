using System;
using Newtonsoft.Json;

namespace NuGetSearch.Models
{
    public class CatalogItem
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

        [JsonProperty(PropertyName = "commitId")]
        public Guid CommitId { get; set; }

        [JsonProperty(PropertyName = "nuget:id")]
        public string PackageName { get; set; }

        [JsonProperty(PropertyName = "nuget:version")]
        public string PackageVersion { get; set; }

        [JsonIgnore]
        public bool IsDelete { 
            get
            {
                return Id == "nuget:PackageDelete";
            }
        }

    }
}
