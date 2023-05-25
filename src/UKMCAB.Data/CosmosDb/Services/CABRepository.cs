using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Net;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Legacy;
using UKMCAB.Data.Storage;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CABRepository : ICABRepository
    {
        private Container _container;
        private readonly CosmosDbConnectionString _cosmosDbConnectionString;
        private readonly IFileStorage _fileStorage;

        public CABRepository(CosmosDbConnectionString cosmosDbConnectionString, IFileStorage fileStorage)
        {
            _cosmosDbConnectionString = cosmosDbConnectionString;
            _fileStorage = fileStorage;
        }

        public async Task InitialiseAsync(bool force = false)//(IFileStorage fileStorage)
        {
            var client = new CosmosClient(_cosmosDbConnectionString);
            var database = client.GetDatabase(DataConstants.CosmosDb.Database);
            if (force)
            {
                var container = database.GetContainer(DataConstants.CosmosDb.Container);
                if (container != null)
                {
                    await container.DeleteContainerAsync();
                    await Task.Delay(2000);
                }
            }
            var result = await database.CreateContainerIfNotExistsAsync(DataConstants.CosmosDb.Container, "/CABId");
            
            if(result.StatusCode == HttpStatusCode.Created)
            {
                _container = result.Container;
                var legacyContainer = database.GetContainer(DataConstants.CosmosDb.ImportContainer);
                await _fileStorage.DropAndRebuildContainer();
                var items = await Query<CABDocument>(legacyContainer, document => true);
                foreach (var cabDocument in items)
                {
                    var id = Guid.NewGuid().ToString();
                    var document = new Document
                    {
                        CABId = id,
                        Name = cabDocument.Name,
                        
                        AddressLine1 = cabDocument.AddressLine1.Clean(),
                        AddressLine2 = cabDocument.AddressLine2.Clean(),
                        TownCity = cabDocument.TownCity.Clean(),
                        Postcode = cabDocument.Postcode.Clean(),
                        County = cabDocument.County.Clean(),
                        Country = cabDocument.Country.Clean(),

                        Email = cabDocument.Email,
                        Website = cabDocument.Website,
                        Phone = cabDocument.Phone,
                        CABNumber = cabDocument.BodyNumber,
                        BodyTypes = SanitiseLists(cabDocument.BodyTypes, DataConstants.Lists.BodyTypes) ,
                        RegisteredOfficeLocation = string.IsNullOrWhiteSpace(cabDocument.RegisteredOfficeLocation) ? string.Empty : SanitiseLists(new List<string> {cabDocument.RegisteredOfficeLocation}, DataConstants.Lists.Countries).First(),
                        TestingLocations = SanitiseLists(cabDocument.TestingLocations, DataConstants.Lists.Countries),
                        LegislativeAreas = SanitiseLists(cabDocument.LegislativeAreas, DataConstants.Lists.LegislativeAreas),
                        HiddenText = cabDocument.HiddenText,
                        Schedules = await ImportSchedules(cabDocument.PDFs, id),
                        Documents = new List<FileUpload>(),
                        Created = new Audit
                        {
                            UserId = "00000000-0000-0000-0000-000000000000",
                            UserName = "Data import",
                            DateTime = DateTime.UtcNow
                        },
                        LastUpdated = new Audit
                        {
                            UserId = "00000000-0000-0000-0000-000000000000",
                            UserName = "Data import",
                            DateTime = cabDocument.LastUpdatedDate.HasValue
                                ? cabDocument.LastUpdatedDate.Value.DateTime
                                : DateTime.UtcNow
                        },
                        Published = new Audit
                        {
                            UserId = "00000000-0000-0000-0000-000000000000",
                            UserName = "Data import",
                            DateTime = cabDocument.PublishedDate.HasValue
                                ? cabDocument.PublishedDate.Value.DateTime
                                : DateTime.UtcNow
                        },
                        StatusValue = Status.Published,
                        RandomSort = Guid.NewGuid().ToString(),
                        LegacyCabId = cabDocument.Id
                    };
                    var newDoc = await CreateAsync(document);
                }
            }

            _container = result.Container;
        }

        private async Task<List<FileUpload>> ImportSchedules(List<PDF> pdfs, string cabId)
        {
            var schedules = new List<FileUpload>();
            if (pdfs == null || pdfs.Count == 0)
            {
                return schedules;
            }

            foreach (var pdf in pdfs)
            {
                var legacyblobStream = await _fileStorage.GetLegacyBlogStream(pdf.BlobName);
                if (legacyblobStream != null)
                {
                    schedules.Add(await _fileStorage.UploadCABFile(cabId, pdf.Label, pdf.ClientFileName, DataConstants.Storage.Schedules, legacyblobStream, "application/pdf"));
                }
            }

            return schedules;
        }

        private static List<string> SanitiseLists(List<string> importData, List<string> masterList)
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
