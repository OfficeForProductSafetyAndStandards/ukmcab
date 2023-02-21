using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.VisualBasic;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private SearchClient _indexClient;
        private readonly int SearchResultPerPage;

        public SearchService(CognitiveSearchConnectionString connectionString)
        {
            var client = new SearchIndexClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));
            _indexClient = client.GetSearchClient("ukmcab-index");
            SearchResultPerPage = 20;
        }

        public async Task<CABResults> QueryAsync(CABSearchOptions options)
        {
            var results = await _indexClient.SearchAsync<CABResult>("*", new SearchOptions
            {
                Size = SearchResultPerPage,
                IncludeTotalCount = true,
                Skip = SearchResultPerPage * (options.PageNumber - 1)
            });

            var cabResults = results.Value.GetResults().ToList();
            var cabs = cabResults.Select(r => r.Document).ToList();
            return new CABResults
            {
                CABs = cabs,
                PageNumber = options.PageNumber,
                Total = Convert.ToInt32(results.Value?.TotalCount.Value ?? 0)
            };
        }
    }
}
