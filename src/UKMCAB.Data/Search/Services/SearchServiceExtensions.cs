using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using OpenSearch.Net;
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
            services.AddSingleton<ISearchService>(x=>new SearchService(searchIndexClient.GetSearchClient(DataConstants.Search.SEARCH_INDEX)));
            services.AddTransient<ISearchServiceManagment, PostgreSearchServiceManagment>();
            services.AddSingleton<ICachedSearchService, CachedSearchService>();

            services.AddSingleton<IOpenSearchClient>( sp =>
            {

                var uri = new Uri("http://ukmcab-opensearch:9200/");

                var connectionPool = new SingleNodeConnectionPool(uri);

                var indexName = DataConstants.Search.SEARCH_INDEX;

                var settings = new OpenSearch.Client.ConnectionSettings(connectionPool)
                    .DefaultIndex(indexName);

                return new OpenSearchClient(settings);
                  
            });
            services.AddSingleton<IOpenSearchIndexerClient, OpenSearchIndexerClient>();
        }
    }
}
