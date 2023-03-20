using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Net;
using UKMCAB.Core.Models;
using UKMCAB.Core.Models.Legacy;
using UKMCAB.Core.Services;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CABRepository: ICABRepository
    {
        private Container _container;

        public CABRepository(string cosmosConnectionString)
        {
            Initialise(new CosmosClient(cosmosConnectionString));
        }

        private void Initialise(CosmosClient client)
        {
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            var result = database.CreateContainerIfNotExistsAsync(DataConstants.CosmosDb.Constainer, "/CABId").Result;
            
            if(result.StatusCode == HttpStatusCode.Created)
            {
                _container = result.Container;
                var legacyContainer = database.GetContainer(DataConstants.CosmosDb.ImportContainer);
                var items = Query<CABDocument>(legacyContainer, document => true).Result;
                foreach (var cabDocument in items)
                {
                    var document = new Document
                    {
                        CABId = Guid.NewGuid().ToString(),

                        Name = cabDocument.Name,
                        Address = cabDocument.Address,
                        Email = cabDocument.Email,
                        Website = cabDocument.Website,
                        Phone = cabDocument.Phone,
                        BodyNumber = cabDocument.BodyNumber,
                        BodyTypes = cabDocument.BodyTypes,
                        RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                        TestingLocations = cabDocument.TestingLocations,
                        LegislativeAreas = cabDocument.LegislativeAreas,
                        HiddenText = cabDocument.LegacyHiddenText,
                        Schedules = cabDocument.PDFs != null && cabDocument.PDFs.Any() ? cabDocument.PDFs.Select(p => new FileUpload { BlobName  = p.BlobName, FileName = p.ClientFileName, UploadDateTime = DateTime.UtcNow}).ToList() : new List<FileUpload>(),
                        Documents = new List<FileUpload>(),
                        PublishedDate = cabDocument.PublishedDate.HasValue
                            ? cabDocument.PublishedDate.Value.DateTime
                            : DateTime.UtcNow,
                        PublishedBy = "admin",
                        LastModifiedDate = cabDocument.LastUpdatedDate.HasValue
                            ? cabDocument.LastUpdatedDate.Value.DateTime
                            : DateTime.UtcNow,
                        LastModifiedBy = "admin",
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "admin",
                        IsLatest = true,
                        IsPublished = true,
                        RandomSort = Guid.NewGuid().ToString()
                    };
                    var newDoc = CreateAsync(document).Result;
                }
            }

            _container = result.Container;
        }

        public async Task<T> GetByIdAsync<T>(string id, string partitionKey)
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(partitionKey));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Resource;
            }

            return default(T);
        }

        //public async Task<Document> GetByIdAsync(string id)
        //{
        //    var response = await _container.ReadItemAsync<Document>(id, new PartitionKey(id));
        //    if (response.StatusCode == HttpStatusCode.OK && response.Resource.id == id)
        //    {
        //        return response.Resource;
        //    }
        //    return null;
        //}
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

        //public async Task<List<Document>> Query(string whereClause)
        //{
        //    var queryBuilder = new StringBuilder();
        //    queryBuilder.Append("SELECT * ");
        //    queryBuilder.Append("FROM c ");
        //    queryBuilder.Append($"WHERE {whereClause}");

        //    var queryText = queryBuilder.ToString();
        //    var query = _container.GetItemQueryIterator<Document>(new QueryDefinition(queryText));

        //    var list = new List<Document>();
        //    while (query.HasMoreResults)
        //    {
        //        var response = await query.ReadNextAsync();
        //        list.AddRange(response.Resource.Select(r => r));
        //    }

        //    return list;
        //}
        public async Task<List<T>> Query<T>(Expression<Func<T, bool>> predicate)
        {
            var query = _container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();
            var list = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                list.AddRange(response.Resource.Select(r => r));
            }

            return list;
        }
        public async Task<List<T>> Query<T>(Container container, Expression<Func<T, bool>> predicate)
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

        public async Task<bool> Updated(Document document)
        {
            var response = await _container.UpsertItemAsync(document);
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> Update(dynamic document)
        {
            var response = await _container.UpsertItemAsync(document);
            return response.StatusCode == HttpStatusCode.OK;
        }

    }

}
