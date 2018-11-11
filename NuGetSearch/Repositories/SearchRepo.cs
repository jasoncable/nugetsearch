using System;
using Nest;
using NuGetSearch.Models;

namespace NuGetSearch.Repositories
{
    public class SearchRepo : ISearchRepo
    {
        private Uri _nodeLocation;
        private ConnectionSettings _settings;
        private IndexNameResolver _resolver;
        private string _indexName;
        private ElasticClient _client;

        public SearchRepo()
        {
        }

        public void Initialize(NuGetSearchAppConfig config)
        {
            _nodeLocation = new Uri(config.ServerUri);
            _settings = new ConnectionSettings(_nodeLocation)
                .DefaultMappingFor<NuGetSearchMain>(m => m.IndexName(config.IndexName)
                    .IdProperty("Id"));
            _resolver = new IndexNameResolver(_settings);
            _indexName = _resolver.Resolve<NuGetSearchMain>();
            _client = new ElasticClient(_settings);
        }

        public NuGetSearchMainSearchResult DoSearch(string searchString, int page)
        {
            NuGetSearchMainSearchRequest request = new NuGetSearchMainSearchRequest
            {
                MaxResults = 1000,
                Page = page,
                PageSize = 20,
                SearchString = searchString
            };

            return NuGetSearchMainRepo.DoSearch(_client, request);
        }
    }
}
