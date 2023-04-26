using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using System.Net;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Legacy;

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

        public async Task InitialiseAsync()
        {
            var client = new CosmosClient(_cosmosDbConnectionString);
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            var result = await database.CreateContainerIfNotExistsAsync(DataConstants.CosmosDb.Constainer, "/CABId");
            
            if(result.StatusCode == HttpStatusCode.Created)
            {
                _container = result.Container;
                var legacyContainer = database.GetContainer(DataConstants.CosmosDb.ImportContainer);
                var items = await Query<CABDocument>(legacyContainer, document => true);
                foreach (var cabDocument in items)
                {
                    var document = new Document
                    {
                        CABId = Guid.NewGuid().ToString(),

                        Name = cabDocument.Name,
                        AddressLine1 = cabDocument.Address,
                        Email = cabDocument.Email,
                        Website = cabDocument.Website,
                        Phone = cabDocument.Phone,
                        CABNumber = cabDocument.BodyNumber,
                        BodyTypes = SanitiseLists(cabDocument.BodyTypes, DataConstants.Lists.BodyTypes) ,
                        RegisteredOfficeLocation = string.IsNullOrWhiteSpace(cabDocument.RegisteredOfficeLocation) ? string.Empty : SanitiseLists(new List<string> {cabDocument.RegisteredOfficeLocation}, DataConstants.Lists.Countries).First(),
                        TestingLocations = SanitiseLists(cabDocument.TestingLocations, DataConstants.Lists.Countries),
                        LegislativeAreas = SanitiseLists(cabDocument.LegislativeAreas, DataConstants.Lists.LegislativeAreas),
                        HiddenText = cabDocument.HiddenText,
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
                        StatusValue = Status.Published,
                        RandomSort = Guid.NewGuid().ToString()
                    };
                    var newDoc = await CreateAsync(document);
                }
            }

            _container = result.Container;
        }

        private List<string> SanitiseLists(List<string> importData, List<string> masterList)
        {
            var sanitisedList = new List<string>();
            if (importData == null)
            {
                return null;
            }
            foreach (var data in importData)
            {
                var listItem =
                    masterList.SingleOrDefault(ml => ml.Equals(data, StringComparison.InvariantCultureIgnoreCase));
                if (listItem != null)
                {
                    sanitisedList.Add(listItem);
                }
                else
                {
                    switch (data.ToLower())
                    {
                        case "united states of america":
                            sanitisedList.Add("United States");
                            break;
                        case "recognised third-party organisation":
                            sanitisedList.Add("Recognised third-party");
                            break;
                        case "third-country-body":
                            sanitisedList.Add("Overseas body");
                            break;
                        default:
                            sanitisedList.Add(data);
                            break;
                    }
                }
            }
            return sanitisedList;
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
