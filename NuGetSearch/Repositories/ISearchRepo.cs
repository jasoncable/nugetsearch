using System;
using Nest;
using NuGetSearch.Models;

namespace NuGetSearch.Repositories
{
    public interface ISearchRepo
    {
        void Initialize(NuGetSearchAppConfig config);
        NuGetSearchMainSearchResult DoSearch(string searchString, int page);
    }
}
