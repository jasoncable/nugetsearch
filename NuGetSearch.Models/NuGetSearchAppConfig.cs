using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetSearch.Models
{
    public class NuGetSearchAppConfig
    {
        public string ServerUri { get; set; }
        public string IndexName { get; set; }
        public string CacheDirectory { get; set; }
    }
}
