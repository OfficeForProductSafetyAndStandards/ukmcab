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

        public Task<CAB> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<CAB>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
