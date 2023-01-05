using Microsoft.Azure.Cosmos;
using System.Net;
using System.Text;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CABRepository: ICABRepository
    {
        private Container _container;

        public CABRepository(CosmosClient client, string databaseName, string containeName)
        {
            _container = client.GetContainer(databaseName, containeName);
        }
        public async Task<Document> GetByIdAsync(string id)
        {
            var response = await _container.ReadItemAsync<Document>(id, new PartitionKey(id));
            if (response.StatusCode == HttpStatusCode.OK && response.Resource.id == id)
            {
                return response.Resource;
            }
            return null;
        }
        public async Task<Document> CreateAsync(Document document)
        {
            document.id = Guid.NewGuid().ToString();
            var response = await _container.CreateItemAsync(document);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                return response.Resource;
            }
            return null;
        }

        public async Task<List<Document>> Query(string whereClause)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT * ");
            queryBuilder.Append("FROM c ");
            queryBuilder.Append($"WHERE {whereClause}");

            var queryText = queryBuilder.ToString();
            var query = _container.GetItemQueryIterator<Document>(new QueryDefinition(queryText));
            var list = new List<Document>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource.Select(r => r));
            }

            return list;
        }
    }
}
