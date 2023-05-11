using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Common.ConnectionStrings;

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
            services.AddSingleton<ISearchService>(x=>new SearchService(searchIndexClient.GetSearchClient(DataConstants.Search.SEARCH_INDEX), searchIndexerClient, x.GetRequiredService<TelemetryClient>()));
            services.AddSingleton<SearchServiceManagment>();
            services.AddSingleton<ICachedSearchService, CachedSearchService>();
        }
    }
}
