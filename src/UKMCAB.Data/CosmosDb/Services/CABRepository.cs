using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Net;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CABRepository : ICABRepository
    {
        private Container _container;
        private readonly CosmosDbConnectionString _cosmosDbConnectionString;

        public CABRepository(CosmosDbConnectionString cosmosDbConnectionString)
        {
            _cosmosDbConnectionString = cosmosDbConnectionString;
        }

        private string GetSlug(string cabName, List<string> slugs)
        {
            var slug = Slug.Make(cabName);
            if (slugs.Contains(slug))
            {
                var index = 0;
                var newSlug = $"{slug}{index}";
                while (slugs.Contains(newSlug))
                {
                    newSlug = $"{slug}-{++index}";
                }

                slug = newSlug;
            }
            slugs.Add(slug);
            return slug;
        }

        public async Task<bool> InitialiseAsync(bool force = false)
        {
            var client = new CosmosClient(_cosmosDbConnectionString);
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            _container = database.GetContainer(DataConstants.CosmosDb.Container);
            var items = await Query<Document>(_container, document => true);
            var slugList = new List<string>();
            if (string.IsNullOrWhiteSpace(items.First().URLSlug))
            {
                foreach (var document in items)
                {
                    document.URLSlug = GetSlug(document.Name, slugList);
                    await Update(document);
                }

                force = true;
            }
            return force;
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

        public async Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate)
        {
            return await Query<T>(_container, predicate);
        }

        public IQueryable<Document> GetItemLinqQueryable() => _container.GetItemLinqQueryable<Document>();

        private async Task<List<T>> Query<T>(Container container, Expression<Func<T, bool>> predicate)
        {
            var query = container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();
            var list = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource.Select(r => r));
            }

            return list;
        }

        public async Task<bool> Update(Document document)
        {
            var response = await _container.UpsertItemAsync(document);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> Delete(Document document)
        {
            var response = await _container.DeleteItemAsync<Document>(document.id, new PartitionKey(document.CABId));
            return response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
