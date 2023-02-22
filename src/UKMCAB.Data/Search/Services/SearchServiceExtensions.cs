using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public static class SearchServiceExtensions
    {
        private const string SEARCH_INDEX = "ukmcab-search-index";
        private const string SEARCH_INDEXER = "ukmcab-search-indexer";
        private const string SEARCH_DATASOURCE = "ukmcab-search-datasource";

        public static void AddSearchService(this IServiceCollection services, CognitiveSearchConnectionString connectionString, string cosmosDBConnectionString, int searchResultsPerPage)
        {
            var searchIndexClient = new SearchIndexClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));
            CreateIndex(searchIndexClient);

            var searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint),
                new AzureKeyCredential(connectionString.ApiKey));
            CreateDataSourceAndIndexer(searchIndexerClient, cosmosDBConnectionString);

            services.AddSingleton<ISearchService>(new SearchService(searchIndexClient.GetSearchClient(SEARCH_INDEX), searchResultsPerPage));
        }

        private static void CreateIndex(SearchIndexClient searchIndexClient)
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(CABDocument));
            var definition = new SearchIndex(SEARCH_INDEX, searchFields);
            try
            {
                searchIndexClient.DeleteIndex(definition.Name);
            }
            catch
            {
                // We don't want to throw if this fails as it does not exist
            }
            searchIndexClient.CreateIndex(definition);
        }

        private static void CreateDataSourceAndIndexer(SearchIndexerClient searchIndexerClient, string cosmosDBConnectionString)
        {
            var cosmosDbDataSource = new SearchIndexerDataSourceConnection(SEARCH_DATASOURCE,
                SearchIndexerDataSourceType.CosmosDb, cosmosDBConnectionString + ";Database=main",
                new SearchIndexerDataContainer("cab-data"));
            searchIndexerClient.CreateOrUpdateDataSourceConnection(cosmosDbDataSource);

            var cosmosDbIndexer =
                new SearchIndexer(SEARCH_INDEXER, cosmosDbDataSource.Name, SEARCH_INDEX)
                {
                    Schedule = new IndexingSchedule(TimeSpan.FromDays(1))
                };
            try
            {
                searchIndexerClient.GetIndexer(cosmosDbIndexer.Name);
                searchIndexerClient.ResetIndexer(cosmosDbIndexer.Name);
            }
            catch
            {
                // don't throw if indexer doesn't exist
            }

            searchIndexerClient.CreateOrUpdateIndexer(cosmosDbIndexer);
            searchIndexerClient.RunIndexer(cosmosDbIndexer.Name);
        }
    }
}
