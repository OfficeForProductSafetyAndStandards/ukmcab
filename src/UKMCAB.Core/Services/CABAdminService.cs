using UKMCAB.Common;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data;
using UKMCAB.Data.Search.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Core.Services
{
    public class CABAdminService : ICABAdminService
    {
        private readonly ICABRepository _cabRepostitory;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly TelemetryClient _telemetryClient;

        public CABAdminService(ICABRepository cabRepostitory, ICachedSearchService cachedSearchService, ICachedPublishedCABService cachedPublishedCabService, TelemetryClient telemetryClient)
        {
            _cabRepostitory = cabRepostitory;
            _cachedSearchService = cachedSearchService;
            _cachedPublishedCabService = cachedPublishedCabService;
            _telemetryClient = telemetryClient;
        }

        public async Task<List<Document>> DocumentWithKeyIdentifiersExistsAsync(Document document)
        {
            var documents = await _cabRepostitory.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase) ||
                d.CABNumber.Equals(document.CABNumber) ||
                (!string.IsNullOrWhiteSpace(document.UKASReference) && d.UKASReference.Equals(document.UKASReference))
            );
            return documents.Where(d => !d.CABId.Equals(document.CABId)).ToList();
        }

        public async Task<Document> FindPublishedDocumentByCABIdAsync(string id)
        {
            var doc = await _cabRepostitory.Query<Document>(d => d.StatusValue == Status.Published && d.CABId.Equals(id));
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        public async Task<List<Document>> FindAllDocumentsByCABIdAsync(string id)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.CABId.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        public async Task<List<Document>> FindAllDocumentsByCABURLAsync(string url)
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.URLSlug.Equals(url, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        public async Task<List<Document>> FindAllCABManagementQueueDocuments()
        {
            var docs = await _cabRepostitory.Query<Document>(d =>
                d.StatusValue == Status.Draft || d.StatusValue == Status.Archived);
            return docs;
        }

        public async Task<Document> GetLatestDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // if a newly create cab or a draft version exists this will be the latest version, there should be no more than one
            if (documents.Any(d => d.StatusValue == Status.Created || d.StatusValue == Status.Draft))
            {
                return documents.Single(d => d.StatusValue == Status.Created || d.StatusValue == Status.Draft);
            }
            // if no draft or created version exists then see if a published version exists, again should only ever be one
            if (documents.Any(d => d.StatusValue == Status.Published))
            {
                return documents.Single(d => d.StatusValue == Status.Published);
            }

            return null;
        }

        public IAsyncEnumerable<string> GetAllCabIds()
        {
            return _cabRepostitory.GetItemLinqQueryable().Select(x => x.CABId).AsAsyncEnumerable();
        }

        public async Task<Document> CreateDocumentAsync(UKMCABUser user, Document document, bool saveAsDraft = false)
        {
            var documentExists = await DocumentWithKeyIdentifiersExistsAsync(document);

            Guard.IsFalse(documentExists.Any(), "CAB name or number already exists in database");
            
            var auditItem = new Audit(user);
            document.CABId = Guid.NewGuid().ToString();
            document.Created = auditItem;
            document.LastUpdated = auditItem;
            document.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
            var rv = await _cabRepostitory.CreateAsync(document);

            await RecordStatsAsync();

            return rv;
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(UKMCABUser user, Document draft, bool saveAsDraft = false)
        {
            var audit = new Audit(user);
            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
                draft.id = string.Empty;
                draft.Created = audit;
                draft.LastUpdated = audit;
                var newdraft = await _cabRepostitory.CreateAsync(draft);
                Guard.IsFalse(newdraft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft.CABId}");
                return newdraft;
            }

            if (draft.StatusValue == Status.Created && saveAsDraft)
            {
                draft.StatusValue = Status.Draft;
            }
            if (draft.StatusValue == Status.Created || draft.StatusValue == Status.Draft)
            {
                draft.LastUpdated = audit;
                Guard.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");

                await RecordStatsAsync();

                return draft;
            }

            throw new Exception($"Invalid document for creating or updating draft, CAB Id: {draft.CABId}");
        }

        public async Task<bool> DeleteDraftDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // currently we only delete newly created docs that haven't been saved as draft
            var createdDoc = documents.SingleOrDefault(d => d.StatusValue == Status.Created);
            Guard.IsTrue(createdDoc != null, $"Error finding the document to delete, CAB Id: {cabId}");
            Guard.IsTrue(await _cabRepostitory.Delete(createdDoc), $"Failed to delete draft version, CAB Id: {cabId}");
            await RecordStatsAsync();
            return true;
        }

        public async Task<Document> PublishDocumentAsync(UKMCABUser user, Document latestDocument)
        {
            if (latestDocument.StatusValue == Status.Published)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already published doc.
                return latestDocument;
            }
            Guard.IsTrue(latestDocument.StatusValue == Status.Created || latestDocument.StatusValue == Status.Draft, $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            var audit = new Audit(user);
            if (publishedVersion == null)
            {
                // If there is no published version then we set the published date
                latestDocument.Published = audit;
            }
            else
            {
                publishedVersion.StatusValue = Status.Historical;
                Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(publishedVersion.id);
            }

            latestDocument.LastUpdated = audit;
            latestDocument.StatusValue = Status.Published;
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");

            var urlSlug = publishedVersion != null && !publishedVersion.URLSlug.Equals(latestDocument.URLSlug)
                ? publishedVersion.URLSlug
                : latestDocument.URLSlug;

            await _cachedSearchService.ReIndexAsync(new CABIndexItem
            {
                id = latestDocument.id,
                Name = latestDocument.Name,
                CABId = latestDocument.CABId,
                CABNumber = latestDocument.CABNumber,
                AddressLine1 = latestDocument.AddressLine1,
                AddressLine2 = latestDocument.AddressLine1,
                TownCity = latestDocument.TownCity,
                County = latestDocument.County,
                Postcode = latestDocument.Postcode,
                Country = latestDocument.Country,
                Email = latestDocument.Email,
                Phone = latestDocument.Phone,
                Website = latestDocument.Website,
                BodyTypes = latestDocument.BodyTypes.ToArray(),
                TestingLocations = latestDocument.TestingLocations.ToArray(),
                LegislativeAreas = latestDocument.LegislativeAreas.ToArray(),
                RegisteredOfficeLocation = latestDocument.RegisteredOfficeLocation,
                URLSlug = latestDocument.URLSlug,
                LastUpdatedDate = latestDocument.LastUpdatedDate,
                RandomSort = latestDocument.RandomSort,
            });

            await RefreshCaches(latestDocument.CABId, urlSlug);

            await RecordStatsAsync();

            return latestDocument;
        }

        public async Task<Document> ArchiveDocumentAsync(UKMCABUser user, Document latestDocument, string archiveReason)
        {
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            if (publishedVersion == null)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already archived doc.
                var latest = await GetLatestDocumentAsync(latestDocument.id);
                if (latest.StatusValue == Status.Archived)
                {
                    return latest;
                }
            }
            Guard.IsTrue(publishedVersion != null, $"Submitted document for archiving incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var audit = new Audit(user);
            publishedVersion.StatusValue = Status.Archived;
            publishedVersion.Archived = audit;
            publishedVersion.LastUpdated = audit;
            publishedVersion.ArchivedReason = archiveReason;
            Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                $"Failed to archive published version, CAB Id: {latestDocument.CABId}");

            await _cachedSearchService.RemoveFromIndexAsync(publishedVersion.id);
            await RefreshCaches(latestDocument.CABId, latestDocument.URLSlug);
            await RecordStatsAsync();

            return publishedVersion;
        }

        private async Task RefreshCaches(string cabId, string slug)
        {
            await _cachedSearchService.ClearAsync();
            await _cachedSearchService.ClearAsync(cabId);
            await _cachedPublishedCabService.ClearAsync(cabId, slug);
        }

        public async Task RecordStatsAsync()
        {
            async Task RecordStatAsync(Status status) => _telemetryClient.TrackMetric(string.Format(AiTracking.Metrics.CabsByStatusFormat, status.ToString()), 
                await _cabRepostitory.GetItemLinqQueryable().Where(x => x.StatusValue == status).CountAsync());

            await RecordStatAsync(Status.Unknown);
            await RecordStatAsync(Status.Created);
            await RecordStatAsync(Status.Draft);
            await RecordStatAsync(Status.Published);
            await RecordStatAsync(Status.Archived);
            await RecordStatAsync(Status.Historical);
        }
    }


}
