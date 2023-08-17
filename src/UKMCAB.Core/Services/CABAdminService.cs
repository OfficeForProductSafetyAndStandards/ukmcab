using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;

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
                d.CABNumber.Equals(document.CABNumber) ||
                (!string.IsNullOrWhiteSpace(document.UKASReference) && d.UKASReference.Equals(document.UKASReference))
            );
            return documents.Where(d => !d.CABId.Equals(document.CABId)).ToList();
        }
        public async Task<bool> DocumentWithSameNameExistsAsync(Document document)
        {
            var documents = await _cabRepostitory.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase)
            );
            return documents.Where(d => !d.CABId.Equals(document.CABId)).ToList().Count > 0;
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

        public async Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document, bool saveAsDraft = false)
        {
            var documentExists = await DocumentWithKeyIdentifiersExistsAsync(document);

            Guard.IsFalse(documentExists.Any(), "CAB number already exists in database");
            
            var auditItem = new Audit(userAccount, AuditStatus.Created);
            document.CABId = Guid.NewGuid().ToString();
            document.AuditLog.Add(auditItem);
            document.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
            var rv = await _cabRepostitory.CreateAsync(document);

            await RecordStatsAsync();

            return rv;
        }

        public async Task UpdateSearchIndex(Document document)
        {
            await _cachedSearchService.ReIndexAsync(new CABIndexItem
            {
                id = document.id,
                Status = document.Status,
                StatusValue = document.StatusValue.ToString(),
                Name = document.Name,
                CABId = document.CABId,
                CABNumber = document.CABNumber,
                AddressLine1 = document.AddressLine1,
                AddressLine2 = document.AddressLine1,
                TownCity = document.TownCity,
                County = document.County,
                Postcode = document.Postcode,
                Country = document.Country,
                Email = document.Email,
                Phone = document.Phone,
                Website = document.Website,
                BodyTypes = document.BodyTypes.ToArray(),
                TestingLocations = document.TestingLocations.ToArray(),
                LegislativeAreas = document.LegislativeAreas.ToArray(),
                RegisteredOfficeLocation = document.RegisteredOfficeLocation,
                URLSlug = document.URLSlug,
                LastUpdatedDate = document.LastUpdatedDate,
                RandomSort = document.RandomSort,
            });

        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft, bool saveAsDraft = false)
        {
            if (draft.StatusValue == Status.Published)
            {
                // Need to create new version
                draft.StatusValue = saveAsDraft ? Status.Draft : Status.Created;
                draft.id = string.Empty;
                draft.AuditLog = new List<Audit> { new Audit(userAccount, AuditStatus.Created) };
                draft = await _cabRepostitory.CreateAsync(draft);
                Guard.IsFalse(draft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft.CABId}");
            }
            else if (draft.StatusValue == Status.Created || draft.StatusValue == Status.Draft)
            {
                if (draft.StatusValue == Status.Created && saveAsDraft)
                {
                    draft.StatusValue = Status.Draft;
                }
                var audit = new Audit(userAccount, AuditStatus.Saved);
                draft.AuditLog.Add(audit);
                Guard.IsTrue(await _cabRepostitory.Update(draft), $"Failed to update draft , CAB Id: {draft.CABId}");
            }

            if (draft.StatusValue == Status.Draft)
            {
                await UpdateSearchIndex(draft);
            }

            await RefreshCaches(draft.CABId, draft.URLSlug);

            await RecordStatsAsync();

            return draft;
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

        public async Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument)
        {
            if (latestDocument.StatusValue == Status.Published)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already published doc.
                return latestDocument;
            }
            Guard.IsTrue(latestDocument.StatusValue == Status.Created || latestDocument.StatusValue == Status.Draft, $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");
            var publishedDocument = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            if (publishedDocument != null)
            {
                publishedDocument.StatusValue = Status.Historical;
                publishedDocument.AuditLog.Add(new Audit(userAccount, AuditStatus.NewVersion));
                Guard.IsTrue(await _cabRepostitory.Update(publishedDocument),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(publishedDocument.id);
            }

            latestDocument.StatusValue = Status.Published;
            latestDocument.AuditLog.Add(new Audit(userAccount, AuditStatus.Published));
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");

            var urlSlug = publishedDocument != null && !publishedDocument.URLSlug.Equals(latestDocument.URLSlug)
                ? publishedDocument.URLSlug
                : latestDocument.URLSlug;

            await UpdateSearchIndex(latestDocument);

            await RefreshCaches(latestDocument.CABId, urlSlug);

            await RecordStatsAsync();

            return latestDocument;
        }

        public async Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string archiveReason)
        {
            var publishedVersion = await FindPublishedDocumentByCABIdAsync(CABId);
            if (publishedVersion == null)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already archived doc.
                var latest = await GetLatestDocumentAsync(CABId);
                if (latest.StatusValue == Status.Archived)
                {
                    return latest;
                }
            }
            Guard.IsTrue(publishedVersion != null, $"Submitted document for archiving incorrectly flagged, CAB Id: {CABId}");

            publishedVersion.StatusValue = Status.Archived;
            publishedVersion.AuditLog.Add(new Audit(userAccount, AuditStatus.Archived, archiveReason));
            Guard.IsTrue(await _cabRepostitory.Update(publishedVersion), $"Failed to archive published version, CAB Id: {CABId}");

            await UpdateSearchIndex(publishedVersion);

            await RefreshCaches(publishedVersion.CABId, publishedVersion.URLSlug);

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
