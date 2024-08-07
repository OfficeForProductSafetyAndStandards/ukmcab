using System.Linq.Expressions;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.CosmosDb.Services.CAB
{
    public class CABRepository : ICABRepository
    {
        private Container _container;
        private readonly CosmosDbConnectionString _cosmosDbConnectionString;

        public CABRepository(CosmosDbConnectionString cosmosDbConnectionString)
        {
            _cosmosDbConnectionString = cosmosDbConnectionString;
        }

        public async Task<bool> InitialiseAsync(bool force = false)
        {
            var client = new CosmosClient(_cosmosDbConnectionString);
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            _container = database.GetContainer(DataConstants.CosmosDb.CabContainer);
            var items = await Query<Document>(_container, document => true);
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            if (items.Any() && 
                (force || items.Any(doc => ParseVersion(doc.Version) < ParseVersion(DataConstants.Version.Number))))
            {
                var legislativeAreaContainer = database.GetContainer(DataConstants.CosmosDb.LegislativeAreasContainer);
                var legislativeAreas = await Query<LegislativeArea>(legislativeAreaContainer, x => true);

                // UKAS Reference Import
                var csvFilePath = "ukas-reference-numbers-import.csv";
                var ukasReferences =
                    (File.Exists(csvFilePath)
                        ? File.ReadAllLines(csvFilePath)
                            .Skip(1) 
                            .Select(r => r.Split(","))
                        : Array.Empty<string[]>())
                            .Select(r => new { CabId = r[0], UKASRef = r[1] });

                foreach (var document in items)
                {
                    // UKAS Reference Import
                    if (document.StatusValue == Status.Published || document.StatusValue == Status.Draft)
                    {
                        var re = ukasReferences.FirstOrDefault(e => e.CabId.Equals(document.CABId));
                        if (re != null)
                        {
                            document.UKASReference = re.UKASRef;
                        }
                    }

                    // Archive LAs in Archived CABs 
                    if (document.StatusValue == Status.Archived)
                    {
                        document.DocumentLegislativeAreas.ForEach(la => la.Archived = true);
                    }

                    document.Version = DataConstants.Version.Number;

                    document.Documents?.ForEach(s =>
                    {
                        s.Publication ??= DataConstants.Publications.Private;
                    });

                    await UpdateAsync(document);
                }
            }

            return force;
        }

        public async Task<Document> CreateAsync(Document document, DateTime lastUpdatedDateTime)
        {
            document.id = Guid.NewGuid().ToString();
            document.Version = DataConstants.Version.Number;
            document.LastUpdatedDate = lastUpdatedDateTime;
            var response = await _container.CreateItemAsync(document);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                return response.Resource;
            }

            throw new InvalidOperationException($"Document {document.id} not created");
        }

        public async Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate)
        {
            return await Query<T>(_container, predicate);
        }

        public IQueryable<Document> GetItemLinqQueryable() => _container.GetItemLinqQueryable<Document>();
        public async Task UpdateAsync(Document document)
        {
            document.LastUpdatedDate = DateTime.Now;
            var response = await _container.UpsertItemAsync(document);
            Guard.IsTrue(response.StatusCode == HttpStatusCode.OK,
                $"The CAB document was not updated; http status={response.StatusCode}");
        }

        public async Task<bool> DeleteAsync(Document document)
        {
            var response = await _container.DeleteItemAsync<Document>(document.id, new PartitionKey(document.CABId));
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<int> GetCABCountByStatusAsync(Status status = Status.Unknown)
        {
            var list = _container.GetItemLinqQueryable<Document>().AsQueryable();
            if (status == Status.Unknown)
            {
                return await list.CountAsync();
            }

            return await list.Where(x => x.StatusValue == status).CountAsync();
        }

        public async Task<int> GetCABCountBySubStatusAsync(SubStatus subStatus = SubStatus.None)
        {
            var list = _container.GetItemLinqQueryable<Document>().AsQueryable();
            if (subStatus == SubStatus.None)
            {
                return await list.CountAsync();
            }

            return await list.Where(x => x.SubStatus == subStatus).CountAsync();
        }
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
        
        private Version ParseVersion(string version)
        {
            return Version.TryParse(version.Replace("-", ".").Replace("v", string.Empty), out var semanticVersion) ? semanticVersion : Version.Parse(DataConstants.Version.Number);
        }

    }
}