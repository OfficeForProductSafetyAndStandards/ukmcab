using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using CsvHelper;
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

        private class FileMap
        {
            public string Name { get; set; }
            public string CABNumber { get; set; }
            public string FileName { get; set; }
            public string Label { get; set; }
            public string LegislativeArea { get; set; }

        }

        public async Task<bool> InitialiseAsync(bool force = false)
        {
            var client = new CosmosClient(_cosmosDbConnectionString);
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            _container = database.GetContainer(DataConstants.CosmosDb.Container);
            var items = await Query<Document>(_container, document => true);

            var itemToCheck = items.First(i => i.Schedules != null && i.Schedules.Any());
            if (true || string.IsNullOrWhiteSpace(itemToCheck.Schedules.First().LegislativeArea))
            {
                List<FileMap> fileMapList;
                using (var reader = new StreamReader("legislative-file-lookup.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    fileMapList = csv.GetRecords<FileMap>().ToList();
                }

                foreach (var document in items)
                {
                    if (document.Schedules == null || !document.Schedules.Any())
                    {
                        continue;
                    }
                    var maps = new List<FileMap>();
                    if (!string.IsNullOrWhiteSpace(document.CABNumber))
                    {
                        var cabNumber = document.CABNumber.TrimStart('0');
                        maps = fileMapList.Where(f => f.CABNumber.Equals(cabNumber)).ToList();
                    }
                    if (!maps.Any())
                    {
                        maps = fileMapList.Where(f => f.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    }

                    if (!maps.Any())
                    {
                        continue;
                    }
                    var documentChanged = false;
                    foreach (var documentSchedule in document.Schedules)
                    {
                        var map = maps.SingleOrDefault(m => m.Label == documentSchedule.Label && m.FileName == documentSchedule.FileName);
                        if (map != null)
                        {
                            documentSchedule.LegislativeArea = map.LegislativeArea;
                            documentChanged = true;
                        }
                    }

                    if (documentChanged)
                    {
                        await Update(document);
                    }
                }
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
