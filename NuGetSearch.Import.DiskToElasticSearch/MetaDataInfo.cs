using System;
using System.Collections.Generic;

namespace NuGetSearch.Import.DiskToElasticSearch
{
    public class MetaDataInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Versions { get; set; } = new List<string>();
        public int LatestVersion { get; set; }
    }
}
