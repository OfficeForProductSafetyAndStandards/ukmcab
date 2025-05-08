using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Search.Services;

namespace UKMCAB.Data.Search.Services
{
    public static class SearchServiceExtensions
    {
        public static void AddSearchService(this IServiceCollection services, CognitiveSearchConnectionString connectionString)
        {
            var searchIndexClient = new SearchIndexClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));
            var searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));

            services.AddSingleton(searchIndexClient);
            services.AddSingleton(searchIndexerClient);
            services.AddSingleton<ISearchService>(x=>new SearchService(searchIndexClient.GetSearchClient(DataConstants.Search.SEARCH_INDEX)));
            services.AddSingleton<ISearchServiceManagment, PostgreSearchServiceManagment>();
            services.AddSingleton<ICachedSearchService, CachedSearchService>();

            services.AddSingleton<IOpenSearchIndexerClient, OpenSearchIndexerClient>();
        }
    }
}
