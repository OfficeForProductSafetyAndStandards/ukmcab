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

        public static void AddSearchService(this IServiceCollection services, CognitiveSearchConnectionString connectionString, string cosmosDBConnectionString)
        {
            var searchIndexClient = new SearchIndexClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));
            CreateIndex(searchIndexClient);

            var searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint),
                new AzureKeyCredential(connectionString.ApiKey));
            CreateDataSourceAndIndexer(searchIndexerClient, cosmosDBConnectionString);

            services.AddSingleton<ISearchService>(new SearchService(searchIndexClient.GetSearchClient(DataConstants.Search.SEARCH_INDEX)));
        }

        private static void CreateIndex(SearchIndexClient searchIndexClient)
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(CABIndexItem));
            var definition = new SearchIndex(DataConstants.Search.SEARCH_INDEX, searchFields);
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
            var cosmosDbDataSource = new SearchIndexerDataSourceConnection(DataConstants.Search.SEARCH_DATASOURCE,
                SearchIndexerDataSourceType.CosmosDb, cosmosDBConnectionString + $";Database={DataConstants.CosmosDb.Database}",
                new SearchIndexerDataContainer(DataConstants.CosmosDb.Constainer));
            cosmosDbDataSource.Container.Query = "SELECT * FROM c WHERE c.IsPublished = true";

            searchIndexerClient.CreateOrUpdateDataSourceConnection(cosmosDbDataSource);

            var cosmosDbIndexer =
                new SearchIndexer(DataConstants.Search.SEARCH_INDEXER, cosmosDbDataSource.Name, DataConstants.Search.SEARCH_INDEX)
                {
                    Schedule = new IndexingSchedule(TimeSpan.FromMinutes(10)),
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
