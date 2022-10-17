using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Data.CosmosDb
{
    public class CosmosDbService : ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(CosmosClient client, string databaseName, string containeName)
        {
            _container = client.GetContainer(databaseName, containeName);
        }

        public async Task<string> CreateAsync(CAB cab)
        {
            cab.Id = Guid.NewGuid().ToString();
            var response = await _container.CreateItemAsync(cab, new PartitionKey(cab.Id));
            if (response.StatusCode == HttpStatusCode.Created)
            {
                return response.Resource.Id;
            }

            return string.Empty;
        }

        public async Task<bool> UpdateAsync(CAB cab)
        {
            var response = await _container.UpsertItemAsync(cab, new PartitionKey(cab.Id));
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<CAB?> GetByIdAsync(string id)
        {
            var response = await _container.ReadItemAsync<CAB>(id, new PartitionKey(id));
            if (response.StatusCode == HttpStatusCode.OK && response.Resource.Id == id)
            {
                return response.Resource;
            }

            return null;
        }

        public async Task<List<CAB>> GetPagedCABsAsync(int pageNumber = 1, int pageCount = 10)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            var offset = (pageNumber - 1) * pageCount;
            
            var queryText = $"SELECT * FROM c ORDER BY c.name OFFSET {offset} LIMIT {pageCount}";
            
            
            var query = _container.GetItemQueryIterator<CAB>(new QueryDefinition(queryText));
            var list = new List<CAB>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource);
            }

            return list;
        }

        public async Task<int> GetCABCountAsync()
        {
            var query = _container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");
            var response = await query.ReadNextAsync();
            if (response.StatusCode == HttpStatusCode.OK && response.Resource.Count() == 1)
            {
                return response.Resource.First();
            }
            return 0;
        }
    }
}
