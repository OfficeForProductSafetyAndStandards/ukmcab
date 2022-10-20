using Microsoft.Azure.Cosmos;
using System.Net;

using System.Text;
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

        private async Task<List<CAB>> QueryCABs(string queryText)
        {
            var query = _container.GetItemQueryIterator<CabItem>(new QueryDefinition(queryText));
            var list = new List<CAB>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource.Select(r => r.CAB));
            }

            return list;

        }


        public async Task<List<CAB>> Query(string text, FilterSelections filterSelections)
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT cab ");
            queryBuilder.Append("FROM CABs cab ");
            queryBuilder.Append("JOIN regulation IN cab.regulations ");
            queryBuilder.Append("JOIN product IN regulation.Products ");
            queryBuilder.Append($"WHERE (CONTAINS(cab.name, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(cab.bodyNumber, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(regulation.Name, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(product.Name, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(product.PartName, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(product.ScheduleName, '{text}', true) ");
            queryBuilder.Append($"OR CONTAINS(product.StandardsNumber, '{text}', true))");

            var queryText = queryBuilder.ToString();
            var cabs = await QueryCABs(queryText);
            cabs = ApplyFilters(cabs, filterSelections);
            return cabs;
        }

        private List<CAB> ApplyFilters(List<CAB> cabs, FilterSelections filterSelections)
        {
            if (filterSelections.BodyTypes != null && filterSelections.BodyTypes.Any())
            {
                cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.BodyType) &&
                    filterSelections.BodyTypes.Any(bt =>
                         c.BodyType.Contains(bt, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }
            if (filterSelections.RegisteredOfficeLocations != null && filterSelections.RegisteredOfficeLocations.Any())
            {
                cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.RegisteredOfficeLocation) &&
                    filterSelections.RegisteredOfficeLocations.Any(rol =>
                         c.RegisteredOfficeLocation.Contains(rol, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }
            if (filterSelections.TestingLocations != null && filterSelections.TestingLocations.Any())
            {
                cabs = cabs.Where(c => !string.IsNullOrWhiteSpace(c.TestingLocations) &&
                    filterSelections.TestingLocations.Any(tl =>
                        c.TestingLocations.Contains(tl, StringComparison.InvariantCultureIgnoreCase))).ToList();
            }
            if (filterSelections.Regulations != null && filterSelections.Regulations.Any())
            {
                cabs = cabs.Where(c =>
                    filterSelections.Regulations.Any(fr =>
                        c.Regulations.Any(r => r.Name.Equals(fr, StringComparison.InvariantCultureIgnoreCase)))).ToList();
            }
            return cabs;
        }
    }
}
