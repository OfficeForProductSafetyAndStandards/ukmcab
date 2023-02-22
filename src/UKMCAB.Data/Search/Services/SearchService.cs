using System.Drawing.Imaging;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private SearchClient _indexClient;
        private readonly int SearchResultPerPage;

        public SearchService(SearchClient searchClient, int searhchResultPerPage)
        {
            _indexClient = searchClient;
            SearchResultPerPage = searhchResultPerPage;
        }

        public async Task<CABResults> QueryAsync(CABSearchOptions options)
        {
            var query = string.IsNullOrWhiteSpace(options.Keywords) ? "*" : options.Keywords;
            var search = await _indexClient.SearchAsync<CABDocument>(query, new SearchOptions
            {
                Size = SearchResultPerPage,
                IncludeTotalCount = true,
                Skip = SearchResultPerPage * (options.PageNumber - 1)
            });
            var cabResults = new CABResults
            {
                PageNumber = options.PageNumber,
                Total = 0,
                CABs = new List<CABDocument>()
            };
            if (!search.HasValue)
            {
                return cabResults;
            } 
            
            var results = search.Value.GetResults().ToList();
            if (!results.Any())
            {
                return cabResults;
            }

            var cabs = results.Select(r => r.Document).ToList();
            cabResults.CABs = cabs;
            cabResults.Total = Convert.ToInt32(search.Value.TotalCount);
            return cabResults;
        }
    }
}
