using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NuGetSearch.Models
{
    public class PackageDetail
    {
        [JsonProperty(PropertyName = "@id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "@type")]
        public List<string> Type { get; set; }

        [JsonProperty(PropertyName = "catalog:commitTimeStamp")]
        public DateTime CommitTimeStamp { get; set; }

        [JsonProperty(PropertyName = "catalog:commitId")]
        public Guid CommitId { get; set; }

        [JsonProperty(PropertyName = "authors")]
        public string Authors { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string PackageName { get; set; }

        [JsonProperty(PropertyName = "isPrerelease")]
        public bool IsPrerelease { get; set; }

        [JsonProperty(PropertyName = "published")]
        public DateTime Published { get; set; }

        [JsonProperty(PropertyName = "packageSize")]
        public string PackageSize { get; set; }

        [JsonProperty(PropertyName = "iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty(PropertyName = "dependencyGroups")]
        public List<DependencyGroup> Dependencies { get; set; }

        [JsonProperty(PropertyName = "listed")]
        public bool Listed { get; set; }

        [JsonProperty(PropertyName = "minClientVersion")]
        public string MinClientVersion { get; set; }

        [JsonProperty(PropertyName = "projectUrl")]
        public string ProjectUrl { get; set; }

        [JsonProperty(PropertyName = "releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonProperty(PropertyName = "summary")]
        public string Summary { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<string> Tags { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "verbatimVersion")]
        public string VerbatimVersion { get; set; }

        [JsonProperty(PropertyName = "generatedId")]
        public Guid? GeneratedId { get; set; }


    }
}
