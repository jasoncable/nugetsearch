using System.Collections.Generic;

namespace NuGetSearch.Models
{
    public class NuGetSearchMainDependencyGroup
    {
        public string TargetFramework { get; set; }
        public List<NuGetSearchMainDependency> Dependencies { get; set; }
    }
}