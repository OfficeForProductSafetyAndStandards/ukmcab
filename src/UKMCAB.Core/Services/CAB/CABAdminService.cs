using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Linq;
using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain.CAB;
using UKMCAB.Core.Mappers;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using static System.Formats.Asn1.AsnWriter;

// ReSharper disable SpecifyStringComparison - Not For Cosmos

namespace UKMCAB.Core.Services.CAB
{
    //todo Change these methods to use CabModel as return value / params instead of Document
    public class CABAdminService : ICABAdminService
    {
        private readonly ICABRepository _cabRepository;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly TelemetryClient _telemetryClient;

        public CABAdminService(ICABRepository cabRepository, ICachedSearchService cachedSearchService,
            ICachedPublishedCABService cachedPublishedCabService, TelemetryClient telemetryClient,
            IUserService userService)
        {
            _cabRepository = cabRepository;
            _cachedSearchService = cachedSearchService;
            _cachedPublishedCabService = cachedPublishedCabService;
            _telemetryClient = telemetryClient;
        }

        public async Task<List<CabModel>> FindOtherDocumentsByCabNumberOrUkasReference(string cabId, string? cabNumber,
            string? ukasReference)
        {
            var documents = await _cabRepository.Query<Document>(d =>
                (!string.IsNullOrWhiteSpace(cabNumber) &&
                 d.CABNumber!.Equals(cabNumber)) ||
                (!string.IsNullOrWhiteSpace(ukasReference) && d.UKASReference.Equals(ukasReference)));
            //Do not identify same Cab Id as a duplicate
            var documentsFound = documents.Where(d => !d.CABId.Equals(cabId)).ToList();
            return documentsFound.Select(document => document.MapToCabModel()).ToList();
        }

        public async Task<bool> DocumentWithSameNameExistsAsync(Document document)
        {
            var documents = await _cabRepository.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase)
            );
            return documents.Where(d => !d.CABId.Equals(document.CABId)).ToList().Count > 0;
        }

        public async Task<List<CabModel>> FindDocumentsByCABIdAsync(string id)
        {
            var documents = await FindAllDocumentsByCABIdAsync(id);
            return documents.Select(document => document.MapToCabModel())
                .OrderByDescending(d => d.LastUpdatedUtc)
                .ToList();
        }

        public async Task<List<Document>> FindAllDocumentsByCABURLAsync(string url, Status[]? statusesToRetrieve = null)
        {
            List<Document> docs;
            if (statusesToRetrieve != null && statusesToRetrieve.Any())
            {
                docs = await _cabRepository.Query<Document>(d =>
                    d.URLSlug.Equals(url, StringComparison.CurrentCultureIgnoreCase) &&
                    statusesToRetrieve.Contains(d.StatusValue));
            }
            else
            {
                docs = await _cabRepository.Query<Document>(d =>
                    d.URLSlug.Equals(url, StringComparison.CurrentCultureIgnoreCase));
            }

            return docs;
        }


        /// <inheritdoc />
        public async Task<List<CabModel>> FindAllCABManagementQueueDocumentsForUserRole(string? userRole)
        {
            var docs = new List<Document>();

            if (!string.IsNullOrWhiteSpace(userRole))
            {
                docs = await _cabRepository.Query<Document>(d => (d.CreatedByUserGroup == userRole &&
                                                                  d.StatusValue == Status.Draft) ||
                                                                 d.StatusValue == Status.Archived);

                return docs.Select(document => document.MapToCabModel()).ToList();
            }

            docs = await _cabRepository.Query<Document>(d =>
                d.StatusValue == Status.Draft || d.StatusValue == Status.Archived);
            return docs.Select(document => document.MapToCabModel()).ToList();
        }

        public async Task<Document?> GetLatestDocumentAsync(string cabId)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            // if a newly create cab or a draft version exists this will be the latest version, there should be no more than one
            if (documents.Any(d => d is { StatusValue: Status.Draft }))
            {
                return documents.Single(d => d is { StatusValue: Status.Draft });
            }

            // if no draft or created version exists then see if a published version exists, again should only ever be one
            if (documents.Any(d => d is { StatusValue: Status.Published }))
            {
                return documents.Single(d => d is { StatusValue: Status.Published });
            }

            return null;
        }

        public IAsyncEnumerable<string> GetAllCabIds()
        {
            return _cabRepository.GetItemLinqQueryable().Select(x => x.CABId).AsAsyncEnumerable();
        }

        public async Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document)
        {
            var documentExists =
                await FindOtherDocumentsByCabNumberOrUkasReference(document.CABId, document.CABNumber,
                    document.UKASReference);

            Guard.IsFalse(documentExists.Any(), "CAB number already exists in database");

            var auditItem = new Audit(userAccount, AuditCABActions.Created);
            document.CABId ??= Guid.NewGuid().ToString();
            document.AuditLog.Add(auditItem);
            document.StatusValue = Status.Draft;
            document.CreatedByUserGroup = userAccount.Role!.ToLower();

            var rv = await _cabRepository.CreateAsync(document);
            await UpdateSearchIndex(rv);
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
                SubStatus = ((int)document.SubStatus).ToString(),
                LastUserGroup = document.LastUserGroup,
                CreatedByUserGroup = document.CreatedByUserGroup,
                Name = document.Name,
                CABId = document.CABId,
                CABNumber = document.CABNumber,
                AddressLine1 = document.AddressLine1,
                AddressLine2 = document.AddressLine2,
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
                draft.SubStatus = SubStatus.PendingApprovalToPublish;
                draft.AuditLog.Add(new Audit(userAccount, AuditCABActions.SubmittedForApproval));
            }

            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = Status.Draft;
                draft.AuditLog = new List<Audit> { new(userAccount, AuditCABActions.Created) };
                draft.CreatedByUserGroup = userAccount.Role!.ToLower();
                draft = await _cabRepository.CreateAsync(draft);
            }
            else if (draft.StatusValue == Status.Draft)
            {
                draft.AuditLog.Add(new Audit(userAccount, AuditCABActions.Saved));
                await _cabRepository.UpdateAsync(draft);
            }

            await UpdateSearchIndex(draft);
            await RefreshCaches(draft.CABId, draft.URLSlug);
            await RecordStatsAsync();

            return draft;
        }

        public async Task DeleteDraftDocumentAsync(UserAccount userAccount, Guid cabId, string? deleteReason)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId.ToString());
            var draft = documents.SingleOrDefault(d => d is { StatusValue: Status.Draft });
            if (draft == null)
            {
                // An accidental double submit might cause this action to be repeated so just return without doing anything.
                return;
            }

            var previous = documents.OrderByDescending(x => x.LastUpdatedDate).FirstOrDefault(d => d.id != draft.id);
            Guard.IsTrue(previous == null || !string.IsNullOrEmpty(deleteReason),
                $"The delete reason must be specified when an earlier document version exists.");

            // Delete the draft CAB record.
            Guard.IsTrue(await _cabRepository.DeleteAsync(draft),
                $"Failed to delete draft version, CAB Id: {cabId}");

            // Make updates to previous cab version, if one exists.
            if (previous != null)
            {
                // Add audit log entry to previous version.
                previous.AuditLog.Add(new Audit(userAccount, AuditCABActions.DraftDeleted, deleteReason));

                // Ensure substatus is set to None. Needed when deleting a draft created as a result of a request to unarchive.
                previous.SubStatus = SubStatus.None;

                await _cabRepository.UpdateAsync(previous);
                await UpdateSearchIndex(previous);
            }

            // Update the search index & caches
            await _cachedSearchService.RemoveFromIndexAsync(draft.id);
            await RefreshCaches(draft.CABId, draft.URLSlug);
            await RecordStatsAsync();
        }

        /// <summary>
        /// Updates document with Pending approval with audit action
        /// </summary>
        /// <param name="cabId">cabId to update</param>
        /// <param name="status">status of Cab to get</param>
        /// <param name="subStatus"></param>
        /// <param name="audit">audit log to save</param>
        public async Task SetSubStatusAsync(Guid cabId, Status status, SubStatus subStatus, Audit audit)
        {
            var documents =
                await _cabRepository.Query<Document>(c => c.CABId == cabId.ToString() && c.Status == status.ToString());
            if (documents.Count != 1)
            {
                throw new NotFoundException("Single Document not found to set sub status");
            }

            var document = documents.First();
            document.SubStatus = subStatus;
            document.AuditLog.Add(audit);
            await _cabRepository.UpdateAsync(document);
            await UpdateSearchIndex(document);
            await RefreshCaches(document.CABId, document.URLSlug);
        }

        public async Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument,
            string? publishInternalReason = default(string), string? publishPublicReason = default(string))
        {
            if (latestDocument.StatusValue == Status.Published)
            {
                // An accidental double submit might cause this action to be repeated so just return the already published doc.
                return latestDocument;
            }

            Guard.IsTrue(latestDocument.StatusValue == Status.Draft,
                $"Submitted document for publishing incorrectly flagged, CAB Id: {latestDocument.CABId}");

            var allDocument = await FindAllDocumentsByCABIdAsync(latestDocument.CABId);
            var publishedOrArchivedDocument = allDocument.SingleOrDefault(doc =>
                doc is { StatusValue: Status.Published or Status.Archived });
            if (publishedOrArchivedDocument != null)
            {
                publishedOrArchivedDocument.StatusValue = Status.Historical;
                publishedOrArchivedDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.RePublished));
                await _cabRepository.UpdateAsync(publishedOrArchivedDocument);
                await _cachedSearchService.RemoveFromIndexAsync(publishedOrArchivedDocument.id);
            }

            latestDocument.StatusValue = Status.Published;
            latestDocument.SubStatus = SubStatus.None;
            latestDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.Published, latestDocument,
                publishedOrArchivedDocument, publishInternalReason, publishPublicReason));
            latestDocument.RandomSort = Guid.NewGuid().ToString();
            await _cabRepository.UpdateAsync(latestDocument);

            var urlSlug = publishedOrArchivedDocument != null &&
                          !publishedOrArchivedDocument.URLSlug.Equals(latestDocument.URLSlug)
                ? publishedOrArchivedDocument.URLSlug
                : latestDocument.URLSlug;

            await UpdateSearchIndex(latestDocument);

            await RefreshCaches(latestDocument.CABId, urlSlug);

            await RecordStatsAsync();

            return latestDocument;
        }

        public async Task<Document> UnPublishDocumentAsync(UserAccount userAccount, string cabId,
            string? internalReason)
        {
            var docs = await FindAllDocumentsByCABIdAsync(cabId);
            var publishedVersion = docs.SingleOrDefault(d => d.StatusValue == Status.Published);

            Guard.IsTrue(publishedVersion != null,
                $"Submitted document for unpublishing not found, CAB Id: {cabId}");

            var draft = docs.SingleOrDefault(d => d.StatusValue == Status.Draft);
            if (draft != null)
            {
                Guard.IsTrue(await _cabRepository.DeleteAsync(draft),
                    $"Failed to delete draft version before unpublish, CAB Id: {cabId}");
                await _cachedSearchService.RemoveFromIndexAsync(draft.id);
            }

            publishedVersion!.StatusValue = Status.Historical;
            publishedVersion.SubStatus = SubStatus.None;
            publishedVersion.AuditLog.Add(new Audit(userAccount, AuditCABActions.UnpublishApprovalRequest,
                internalReason));
            await _cabRepository.UpdateAsync(publishedVersion);

            await RefreshCaches(publishedVersion.CABId, publishedVersion.URLSlug);

            await _cachedSearchService.RemoveFromIndexAsync(publishedVersion.id);

            await RecordStatsAsync();
            return publishedVersion;
        }

        public async Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string cabId,
            string? unarchiveInternalReason, string unarchivePublicReason, bool requestedByUkas)
        {
            var documents = await FindAllDocumentsByCABIdAsync(cabId);
            var draft = documents.SingleOrDefault(d => d is { StatusValue: Status.Draft });
            if (draft != null && documents.Any(d => d is { StatusValue: Status.Draft }))
            {
                // An accidental double submit might cause this action to be repeated so just return the already unarchived doc.
                return draft;
            }

            var archivedDoc = documents.SingleOrDefault(d => d is { StatusValue: Status.Archived });
            Guard.IsFalse(archivedDoc == null, $"Failed for find and archived version for CAB id: {cabId}");
            // Flag latest with unarchive audit entry
            archivedDoc!.AuditLog.Add(new Audit(userAccount, AuditCABActions.UnarchivedToDraft, unarchiveInternalReason,
                unarchivePublicReason));
            archivedDoc.SubStatus = SubStatus.None;
            await _cabRepository.UpdateAsync(archivedDoc);
            await UpdateSearchIndex(archivedDoc);

            // Create new draft or publish from latest with unarchive entry and reset audit
            archivedDoc.StatusValue = Status.Draft;
            archivedDoc.SubStatus = SubStatus.None;
            archivedDoc.id = string.Empty;
            archivedDoc.AuditLog = new List<Audit>
            {
                new(userAccount, AuditCABActions.Unarchived)
            };

            // If UKAS made the request to unarchive, set them as CreatedByUserGroup in order to see the draft.
            archivedDoc.CreatedByUserGroup = requestedByUkas ? Roles.UKAS.Id : userAccount.Role!.ToLower();

            archivedDoc = await _cabRepository.CreateAsync(archivedDoc);
            await UpdateSearchIndex(archivedDoc);

            await RefreshCaches(archivedDoc.CABId, archivedDoc.URLSlug);

            await RecordStatsAsync();

            return archivedDoc;
        }

        public async Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId,
            string? archiveInternalReason, string archivePublicReason)
        {
            var docs = await FindAllDocumentsByCABIdAsync(CABId);


            var publishedVersion = docs.SingleOrDefault(d => d.StatusValue == Status.Published);
            if (publishedVersion == null)
            {
                // An accidental double submit might cause this action to be repeated so just return the already archived doc.
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
                Guard.IsTrue(await _cabRepository.DeleteAsync(draft),
                    $"Failed to delete draft version before archive, CAB Id: {CABId}");
                await _cachedSearchService.RemoveFromIndexAsync(draft.id);
            }

            publishedVersion.StatusValue = Status.Archived;
            publishedVersion.SubStatus = SubStatus.None;
            publishedVersion.AuditLog.Add(new Audit(userAccount, AuditCABActions.Archived, archiveInternalReason,
                archivePublicReason));
            await _cabRepository.UpdateAsync(publishedVersion);

            await UpdateSearchIndex(publishedVersion);

            await RefreshCaches(publishedVersion.CABId, publishedVersion.URLSlug);

            await RecordStatsAsync();

            return publishedVersion;
        }

        public async Task RecordStatsAsync()
        {
            async Task RecordStatAsync(Status status) => _telemetryClient.TrackMetric(
                string.Format(AiTracking.Metrics.CabsByStatusFormat, status.ToString()),
                await _cabRepository.GetItemLinqQueryable().Where(x => x.StatusValue == status).CountAsync());

            await RecordStatAsync(Status.Unknown);
            await RecordStatAsync(Status.Draft);
            await RecordStatAsync(Status.Published);
            await RecordStatAsync(Status.Archived);
            await RecordStatAsync(Status.Historical);
        }

        public Task<int> GetCABCountForStatusAsync(Status status = Status.Unknown) =>
            _cabRepository.GetCABCountByStatusAsync(status);

        public Task<int> GetCABCountForSubStatusAsync(SubStatus subStatus = SubStatus.None) =>
            _cabRepository.GetCABCountBySubStatusAsync(subStatus);

        public async Task<DocumentScopeOfAppointment> GetDocumentScopeOfAppointmentAsync(Guid cabId, Guid scopeOfAppointmentId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString());
            return latestDocument?.ScopeOfAppointments.First(s => s.Id == scopeOfAppointmentId) ?? throw new InvalidOperationException();
        }

        public async Task<DocumentLegislativeArea> GetDocumentLegislativeAreaAsync(Guid cabId,
            Guid documentLegislativeAreaId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString());
            return latestDocument?.DocumentLegislativeAreas.First(a => a.Id == documentLegislativeAreaId) ?? throw new InvalidOperationException();
        }

        public async Task<Guid> AddLegislativeAreaAsync(Guid cabId, Guid laToAdd, string laName)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString());
            if (latestDocument == null) throw new InvalidOperationException("No document found");
            if (latestDocument.DocumentLegislativeAreas.Any(l => l.LegislativeAreaId == laToAdd))
                throw new ArgumentException("Legislative id already exists on cab");
            var guid = Guid.NewGuid();
            latestDocument.DocumentLegislativeAreas.Add(new DocumentLegislativeArea
            {
                Id = guid,
                LegislativeAreaId = laToAdd
            });
            latestDocument.LegislativeAreas.Add(laName);
            await _cabRepository.UpdateAsync(latestDocument);
            return guid;
        }

        private async Task<List<Document>> FindAllDocumentsByCABIdAsync(string id)
        {
            List<Document> docs = await _cabRepository.Query<Document>(d =>
                d.CABId.Equals(id, StringComparison.CurrentCultureIgnoreCase));
            return docs;
        }

        private async Task RefreshCaches(string cabId, string slug)
        {
            await _cachedSearchService.ClearAsync();
            await _cachedSearchService.ClearAsync(cabId);
            await _cachedPublishedCabService.ClearAsync(cabId, slug);
        }
    }
}