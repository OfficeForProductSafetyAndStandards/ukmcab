using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.DependencyInjection;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchServiceManagment
    {
        private readonly SearchIndexClient _searchIndexClient;
        private readonly SearchIndexerClient _searchIndexerClient;
        private readonly CosmosDbConnectionString _cosmosDbConnectionString;

        public SearchServiceManagment(SearchIndexClient searchIndexClient, SearchIndexerClient searchIndexerClient, CosmosDbConnectionString cosmosDbConnectionString) 
        {
            _searchIndexClient = searchIndexClient;
            _searchIndexerClient = searchIndexerClient;
            _cosmosDbConnectionString = cosmosDbConnectionString;
        }

        public async Task InitialiseAsync()
        {
            await CreateIndexAsync(_searchIndexClient);
            await CreateDataSourceAndIndexerAsync(_searchIndexerClient, _cosmosDbConnectionString);
        }

        private static async Task CreateIndexAsync(SearchIndexClient searchIndexClient)
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(CABIndexItem));
            var definition = new SearchIndex(DataConstants.Search.SEARCH_INDEX, searchFields);
            try
            {
                await searchIndexClient.DeleteIndexAsync(definition.Name);
            }
            catch
            {
                // We don't want to throw if this fails as it does not exist
            }
            await searchIndexClient.CreateIndexAsync(definition);
        }

        private static async Task CreateDataSourceAndIndexerAsync(SearchIndexerClient searchIndexerClient, string cosmosDBConnectionString)
        {
            var cosmosDbDataSource = new SearchIndexerDataSourceConnection(DataConstants.Search.SEARCH_DATASOURCE,
                SearchIndexerDataSourceType.CosmosDb, cosmosDBConnectionString + $";Database={DataConstants.CosmosDb.Database}",
                new SearchIndexerDataContainer(DataConstants.CosmosDb.Constainer));

            cosmosDbDataSource.Container.Query = "SELECT * FROM c WHERE c.IsPublished = true";

            await searchIndexerClient.CreateOrUpdateDataSourceConnectionAsync(cosmosDbDataSource);

            var cosmosDbIndexer = new SearchIndexer(DataConstants.Search.SEARCH_INDEXER, cosmosDbDataSource.Name, DataConstants.Search.SEARCH_INDEX) { Schedule = new IndexingSchedule(TimeSpan.FromMinutes(10)) };

            try
            {
                await searchIndexerClient.GetIndexerAsync(cosmosDbIndexer.Name);
                await searchIndexerClient.ResetIndexerAsync(cosmosDbIndexer.Name);
            }
            catch
            {
                // don't throw if indexer doesn't exist
            }

            await searchIndexerClient.CreateOrUpdateIndexerAsync(cosmosDbIndexer);
            await searchIndexerClient.RunIndexerAsync(cosmosDbIndexer.Name);
        }
    }

    public static class SearchServiceExtensions
    {
        public static void AddSearchService(this IServiceCollection services, CognitiveSearchConnectionString connectionString)
        {
            var searchIndexClient = new SearchIndexClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));
            var searchIndexerClient = new SearchIndexerClient(new Uri(connectionString.Endpoint), new AzureKeyCredential(connectionString.ApiKey));

            services.AddSingleton(searchIndexClient);
            services.AddSingleton(searchIndexerClient);
            services.AddSingleton<ISearchService>(new SearchService(searchIndexClient.GetSearchClient(DataConstants.Search.SEARCH_INDEX)));
            services.AddSingleton<SearchServiceManagment>();
        }
    }
}
