using System;
using System.Collections.Generic;
using System.Text;

namespace NuGetSearch.Models
{
    public class NuGetSearchMainSearchRequest
    {
        public int PageSize { get; set; } = 10;
        public int MaxResults { get; set; } = 10000;
        public int Page { get; set; }
        public string SearchString { get; set; } = String.Empty;
    }
}
