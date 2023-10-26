using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common;
using UKMCAB.Core.Domain.CAB;
using UKMCAB.Core.Mappers;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;

namespace UKMCAB.Core.Services.CAB
{
    public interface ICABAdminService
    {
        Task<List<Document>> DocumentWithKeyIdentifiersExistsAsync(Document document);
        Task<bool> DocumentWithSameNameExistsAsync(Document document);
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABURLAsync(string id);
        Task<List<Document>> FindAllCABManagementQueueDocuments();
        Task<Document> GetLatestDocumentAsync(string id);
        Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document, bool saveAsDraft = false);
        Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft, bool submitForApproval = false);
        Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument);
        Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string archiveReason);
        Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string CABId, string unarchiveReason);
        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
        Task<int> CABCountAsync(Status status = Status.Unknown);
        Task<int> CABCountAsync(SubStatus subStatus = SubStatus.None);
    }

    public class CABAdminService : ICABAdminService
    {
        private readonly ICABRepository _cabRepostitory;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly TelemetryClient _telemetryClient;

        public CABAdminService(ICABRepository cabRepostitory, ICachedSearchService cachedSearchService,
            ICachedPublishedCABService cachedPublishedCabService, TelemetryClient telemetryClient)
        {
            _cabRepostitory = cabRepostitory;
            _cachedSearchService = cachedSearchService;
            _cachedPublishedCabService = cachedPublishedCabService;
            _telemetryClient = telemetryClient;
        }

        public async Task<List<CabModel>> FindOtherDocumentsByCabNumberOrUkasReference(string cabId, string? cabNumber, string? ukasReference)
        {
                var documents = await _cabRepostitory.Query<Document>(d =>
                d.CABNumber!.Equals(cabNumber)
                ||
                (!string.IsNullOrWhiteSpace(ukasReference) && d.UKASReference.Equals(ukasReference))
            );
            var documentsFound =  documents.Where(d => !d.CABId.Equals(cabId)).ToList();
            List<CabModel> cabs = new List<CabModel>();
            foreach (var document in documentsFound)
            {
                cabs.Add(document.MapToCabModel());
            }

            return cabs;
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
            var doc = await _cabRepostitory.Query<Document>(
                d => d.StatusValue == Status.Published && d.CABId.Equals(id));
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        private async Task<List<Document>> FindAllDocumentsByCABIdAsync(string id)
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
            if (documents.Any(d => d.StatusValue == Status.Draft))
            {
                return documents.Single(d => d.StatusValue == Status.Draft);
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

        public async Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document,
            bool saveAsDraft = false)
        {
            var documentExists = await FindOtherDocumentsByCabNumberOrUkasReference(document.id, document.CABNumber, document.UKASReference);

            Guard.IsFalse(documentExists.Any(), "CAB number already exists in database");

            var auditItem = new Audit(userAccount, AuditCABActions.Created);
            document.CABId ??= Guid.NewGuid().ToString();
            document.AuditLog.Add(auditItem);
            document.StatusValue = Status.Draft;
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
                StatusValue = ((int)document.StatusValue).ToString(),
                LastUserGroup = document.LastUserGroup,
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
                BodyTypes = document.BodyTypes?.ToArray() ?? Array.Empty<string>(),
                TestingLocations = document.TestingLocations?.ToArray() ?? Array.Empty<string>(),
                LegislativeAreas =
                    document.LegislativeAreas?.Where(la => la != null).ToArray() ?? Array.Empty<string>(),
                RegisteredOfficeLocation = document.RegisteredOfficeLocation,
                URLSlug = document.URLSlug,
                LastUpdatedDate = document.LastUpdatedDate,
                RandomSort = document.RandomSort,
            });
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft,
            bool submitForApproval = false)
        {
            if (submitForApproval)
            {
                draft.SubStatus = SubStatus.PendingApproval;
            }

            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = Status.Draft;
                draft.AuditLog = new List<Audit> { new Audit(userAccount, AuditCABActions.Created) };
                draft = await _cabRepostitory.CreateAsync(draft);
                Guard.IsFalse(draft == null,
                    $"Failed to create draft version during draft update, CAB Id: {draft?.CABId}");
            }
            else if (draft.StatusValue == Status.Draft)
            {
                draft.AuditLog.Add(new Audit(userAccount, AuditCABActions.Saved));
                await _cabRepostitory.UpdateAsync(draft);
            }

            await UpdateSearchIndex(draft);
            await RefreshCaches(draft.CABId, draft.URLSlug);
            await RecordStatsAsync();

            if (submitForApproval)
            {
                // todo: create task and notify; out-of-scope of ticket 1017
            }

            return draft;
        }

        public async Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument)
        {
            if (latestDocument.StatusValue == Status.Published)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already published doc.
                return latestDocument;
            }

            Guard.IsTrue(latestDocument.StatusValue == Status.Draft,
                $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");

            //var publishedDocument = await FindPublishedDocumentByCABIdAsync(latestDocument.CABId);
            var allDocument = await FindAllDocumentsByCABIdAsync(latestDocument.CABId);
            var publishedOrArchivedDocument = allDocument.SingleOrDefault(doc =>
                doc.StatusValue == Status.Published || doc.StatusValue == Status.Archived);
            if (publishedOrArchivedDocument != null)
            {
                publishedOrArchivedDocument.StatusValue = Status.Historical;
                publishedOrArchivedDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.RePublished));
                Guard.IsTrue(await _cabRepostitory.Update(publishedOrArchivedDocument),
                    $"Failed to update published version during draft publish, CAB Id: {latestDocument.CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(publishedOrArchivedDocument.id);
            }

            latestDocument.StatusValue = Status.Published;
            latestDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.Published, latestDocument,
                publishedOrArchivedDocument));
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            Guard.IsTrue(await _cabRepostitory.Update(latestDocument),
                $"Failed to publish latest version during draft publish, CAB Id: {latestDocument.CABId}");

            var urlSlug = publishedOrArchivedDocument != null &&
                          !publishedOrArchivedDocument.URLSlug.Equals(latestDocument.URLSlug)
                ? publishedOrArchivedDocument.URLSlug
                : latestDocument.URLSlug;

            await UpdateSearchIndex(latestDocument);

            await RefreshCaches(latestDocument.CABId, urlSlug);

            await RecordStatsAsync();

            return latestDocument;
        }

        public async Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string cabId,
            string unarchiveReason)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            var draft = documents.SingleOrDefault(d => d.StatusValue == Status.Draft);
            if (draft != null && documents.Any(d => d.StatusValue == Status.Draft))
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already unarchived doc.
                return draft;
            }

            var archvivedDoc = documents.SingleOrDefault(d => d.StatusValue == Status.Archived);
            Guard.IsFalse(archvivedDoc == null, $"Failed for find and archived version for CAB id: {cabId}");
            // Flag latest with unarchive audit entry
            archvivedDoc.AuditLog.Add(new Audit(userAccount, AuditCABActions.UnarchiveRequest, unarchiveReason));
            Guard.IsTrue(await _cabRepostitory.Update(archvivedDoc),
                $"Failed to update published version during draft publish, CAB Id: {archvivedDoc.CABId}");
            await UpdateSearchIndex(archvivedDoc);

            // Create new draft from latest with unarchive entry and reset audit
            archvivedDoc.StatusValue = Status.Draft;
            archvivedDoc.id = string.Empty;
            archvivedDoc.AuditLog = new List<Audit>
            {
                new Audit(userAccount, AuditCABActions.Unarchived)
            };
            archvivedDoc = await _cabRepostitory.CreateAsync(archvivedDoc);
            Guard.IsFalse(archvivedDoc == null,
                $"Failed to create draft version during unarchive action, CAB Id: {archvivedDoc.CABId}");
            await UpdateSearchIndex(archvivedDoc);

            await RefreshCaches(archvivedDoc.CABId, archvivedDoc.URLSlug);

            await RecordStatsAsync();

            return archvivedDoc;
        }

        public async Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string archiveReason)
        {
            var docs = await FindAllDocumentsByCABIdAsync(CABId);


            var publishedVersion = docs.SingleOrDefault(d => d.StatusValue == Status.Published);
            if (publishedVersion == null)
            {
                // An accidental double sumbmit might cause this action to be repeated so just return the already archived doc.
                var latest = await GetLatestDocumentAsync(CABId);
                if (latest == null || latest.StatusValue == Status.Archived)
                {
                    return latest;
                }
            }

            Guard.IsTrue(publishedVersion != null,
                $"Submitted document for archiving incorrectly flagged, CAB Id: {CABId}");

            var draft = docs.SingleOrDefault(d => d.StatusValue == Status.Draft);
            if (draft != null)
            {
                Guard.IsTrue(await _cabRepostitory.Delete(draft),
                    $"Failed to delete draft version before archive, CAB Id: {CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(draft.id);
            }

            publishedVersion.StatusValue = Status.Archived;
            publishedVersion.AuditLog.Add(new Audit(userAccount, AuditCABActions.Archived, archiveReason));
            Guard.IsTrue(await _cabRepostitory.Update(publishedVersion),
                $"Failed to archive published version, CAB Id: {CABId}");

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
            async Task RecordStatAsync(Status status) => _telemetryClient.TrackMetric(
                string.Format(AiTracking.Metrics.CabsByStatusFormat, status.ToString()),
                await _cabRepostitory.GetItemLinqQueryable().Where(x => x.StatusValue == status).CountAsync());

            await RecordStatAsync(Status.Unknown);
            await RecordStatAsync(Status.Draft);
            await RecordStatAsync(Status.Published);
            await RecordStatAsync(Status.Archived);
            await RecordStatAsync(Status.Historical);
        }

        public Task<int> CABCountAsync(Status status = Status.Unknown) => _cabRepostitory.CABCountAsync(status);


        public Task<int> CABCountAsync(SubStatus subStatus = SubStatus.None) => _cabRepostitory.CABCountAsync(subStatus);
    }
}