using System.Linq.Expressions;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;

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
            _container = database.GetContainer(DataConstants.CosmosDb.Container);
            var items = await Query<Document>(_container, document => true);

            if (items != null && items.Any() &&
                items.Any(doc => !doc.Version?.Equals(DataConstants.Version.Number) ?? true))
            {
                foreach (var document in items)
                {
                    document.Version = DataConstants.Version.Number;
                    await Update(document);
                }
            }

            await UpdateCreatedByUserGroupForCABsInReleaseV2_3(items);

            return force;
        }

        private async Task UpdateCreatedByUserGroupForCABsInReleaseV2_3(List<Document>? items)
        {
            if (items != null && items.Any() &&
                            DataConstants.Version.Number.Equals("v2-3"))
            {
                foreach (var document in items)
                {
                    if (document.AuditLog != null && document.AuditLog.Any())
                    {
                        await UpdateCreatedByUserGroup(document);
                    }
                }
            }
        }

        private async Task UpdateCreatedByUserGroup(Document document)
        {
            foreach (var auditLog in document.AuditLog)
            {
                if (auditLog.Action == AuditCABActions.Created)
                {
                    document.CreatedByUserGroup = auditLog.UserRole;
                    await UpdateAsync(document);
                }
            }
        }

        public async Task<Document> CreateAsync(Document document)
        {
            document.id = Guid.NewGuid().ToString();
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

        [Obsolete("Use " + nameof(UpdateAsync))]
        public async Task<bool> Update(Document document)
        {
            var response = await _container.UpsertItemAsync(document);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task UpdateAsync(Document document)
        {
            var response = await _container.UpsertItemAsync(document);
            Guard.IsTrue(response.StatusCode == HttpStatusCode.OK,
                $"The CAB document was not updated; http status={response.StatusCode}");
        }

        public async Task<bool> Delete(Document document)
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
    }
}