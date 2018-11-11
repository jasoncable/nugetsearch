using System;
using System.Collections.Generic;

namespace NuGetSearch.Models
{
    public class NuGetSearchMainSearchResult
    {
        public string SearchInput { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
        public List<NuGetSearchMain> Data = new List<NuGetSearchMain>();
        public TimeSpan QueryTime { get; set; }
    }
}
