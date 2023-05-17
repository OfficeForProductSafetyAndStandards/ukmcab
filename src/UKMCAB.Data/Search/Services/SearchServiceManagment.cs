using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;
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
            var indexes = await _searchIndexClient.GetIndexesAsync().ToArrayAsync();
            if (!indexes.Any(x => x.Name == DataConstants.Search.SEARCH_INDEX))
            {
                await CreateIndexAsync(_searchIndexClient);
                await CreateDataSourceAndIndexerAsync(_searchIndexerClient, _cosmosDbConnectionString);
            }
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
                new SearchIndexerDataContainer(DataConstants.CosmosDb.Container));

            cosmosDbDataSource.Container.Query = $"SELECT * FROM c WHERE c.Status = \"{Status.Published}\"";

            await searchIndexerClient.CreateOrUpdateDataSourceConnectionAsync(cosmosDbDataSource);

            var cosmosDbIndexer =
                new SearchIndexer(DataConstants.Search.SEARCH_INDEXER, cosmosDbDataSource.Name, DataConstants.Search.SEARCH_INDEX)
                {
                    Schedule = new IndexingSchedule(TimeSpan.FromDays(1))
                    {
                        StartTime = DateTime.Today.Date + new TimeSpan(1,3,0,0) // Start the reindex schedule from tomorrow at 3.00AM
                    }
                };

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
}
