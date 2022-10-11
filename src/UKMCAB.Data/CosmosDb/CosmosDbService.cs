using System.Net;
using Microsoft.Azure.Cosmos;
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

        public async Task<List<CAB>> GetAllAsync()
        {
            var query = _container.GetItemQueryIterator<CAB>(new QueryDefinition("SELECT * FROM c"));
            var list = new List<CAB>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource);
            }

            return list;
        }
    }
}
