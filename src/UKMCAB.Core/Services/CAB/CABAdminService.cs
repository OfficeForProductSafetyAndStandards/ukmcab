using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Linq;
using MoreLinq;
using System.Collections.Generic;
using UKMCAB.Common;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Domain;
using UKMCAB.Core.Security;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.Storage;

// ReSharper disable SpecifyStringComparison - Not For Cosmos

namespace UKMCAB.Core.Services.CAB
{
    //todo Change these methods to use CabModel as return value / params instead of Document
    public class CABAdminService : ICABAdminService
    {
        private readonly ICABRepository _cabRepository;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly ILegislativeAreaService _legislativeAreaService;
        private readonly TelemetryClient _telemetryClient;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;

        public CABAdminService(ICABRepository cabRepository, ICachedSearchService cachedSearchService,
            ICachedPublishedCABService cachedPublishedCabService, ILegislativeAreaService legislativeAreaService, IFileStorage fileStorage,
            TelemetryClient telemetryClient,
            IMapper mapper)
        {
            _cabRepository = cabRepository;
            _cachedSearchService = cachedSearchService;
            _cachedPublishedCabService = cachedPublishedCabService;
            _legislativeAreaService = legislativeAreaService;
            _telemetryClient = telemetryClient;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<List<Document>> FindOtherDocumentsByCabNumberOrUkasReference(string cabId, string? cabNumber,
            string? ukasReference)
        {
            var documents = await _cabRepository.Query<Document>(d =>
                (!string.IsNullOrWhiteSpace(cabNumber) &&
                 d.CABNumber!.Equals(cabNumber)) ||
                (!string.IsNullOrWhiteSpace(ukasReference) && d.UKASReference.Equals(ukasReference)));
            //Do not identify same Cab Id as a duplicate
            return documents.Where(d => !d.CABId.Equals(cabId)).ToList();
        }

        public async Task<bool> DocumentWithSameNameExistsAsync(Document document)
        {
            var documents = await _cabRepository.Query<Document>(d =>
                d.Name.Equals(document.Name, StringComparison.InvariantCultureIgnoreCase)
            );
            return documents.Where(d => !d.CABId.Equals(document.CABId)).ToList().Count > 0;
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
        public async Task<CabManagementDetailsModel> FindAllCABManagementQueueDocumentsForUserRole(string? userRole)
        {
            List<Document> allCabs = await _cabRepository.Query<Document>(d =>
                     (userRole == Roles.OPSS.Id || d.CreatedByUserGroup == userRole) &&
                     (d.StatusValue == Status.Draft ||
                      d.StatusValue == Status.Published && d.SubStatus == SubStatus.PendingApprovalToUnpublish ||
                      d.StatusValue == Status.Published && d.SubStatus == SubStatus.PendingApprovalToArchive ||
                      d.StatusValue == Status.Archived && d.SubStatus == SubStatus.PendingApprovalToUnarchive ||
                      d.StatusValue == Status.Archived && d.SubStatus == SubStatus.PendingApprovalToUnarchivePublish));

            var model = new CabManagementDetailsModel
            {
                AllCabs = allCabs,
                DraftCabs = allCabs.Where(cab => cab.SubStatus == SubStatus.None).ToList(),
                PendingDraftCabs = allCabs.Where(cab => cab.SubStatus == SubStatus.PendingApprovalToUnarchive || cab.SubStatus == SubStatus.PendingApprovalToUnpublish).ToList(),
                PendingPublishCabs = allCabs.Where(cab => cab.SubStatus == SubStatus.PendingApprovalToPublish || cab.SubStatus == SubStatus.PendingApprovalToUnarchivePublish).ToList(),
                PendingArchiveCabs = allCabs.Where(cab => cab.SubStatus == SubStatus.PendingApprovalToArchive).ToList(),
            };

            return model;
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

        public async Task FilterCabContentsByLaIfPendingOgdApproval(Document latestDocument, string userRoleId)
        {
            // Check if the CAB is pending OGD approval. If so, only display data for the LAs that the current user's role is linked to.
            if (latestDocument.IsPendingOgdApproval)
            {
                latestDocument.DocumentLegislativeAreas.RemoveAll(la => la.RoleId != userRoleId);

                var ogdLegislativeAreas = await _legislativeAreaService.GetLegislativeAreasByRoleId(userRoleId);
                var ogdLaNames = ogdLegislativeAreas.Select(la => la.Name).ToList();
                latestDocument.Schedules.RemoveAll(s => s.LegislativeArea != null && !ogdLaNames.Contains(s.LegislativeArea));
            }
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

            if (!document.DocumentLegislativeAreas.Any(la => la.Status == LAStatus.PendingApproval || la.Status == LAStatus.Declined || la.Status == LAStatus.DeclinedByOpssAdmin))
            {
                document.CreatedByUserGroup = userAccount.Role!.ToLower();
            }   

            var rv = await _cabRepository.CreateAsync(document, auditItem.DateTime);
            await UpdateSearchIndexAsync(rv);
            await RecordStatsAsync();

            return rv;
        }

        public async Task UpdateSearchIndexAsync(Document document)
        {
            await _cachedSearchService.ReIndexAsync(new CABIndexItem
            {
                Id = document.id,
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
                RegisteredOfficeLocation = document.RegisteredOfficeLocation,
                URLSlug = document.URLSlug,
                LastUpdatedDate = document.LastUpdatedDate,
                RandomSort = document.RandomSort,
                UKASReference = document.UKASReference,
                HiddenScopeOfAppointments = document.HiddenScopeOfAppointments.ToArray(),
                DocumentLegislativeAreas =
                    _mapper.Map<List<DocumentLegislativeAreaIndexItem>>(document.DocumentLegislativeAreas)
            });
        }

        public async Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft,
            bool submitForApproval = false)
        {
            if (submitForApproval)
            {
                draft.SubStatus = SubStatus.PendingApprovalToPublish;
                draft.AuditLog.Add(new Audit(userAccount, AuditCABActions.SubmittedForApproval));
                draft.DocumentLegislativeAreas.Where(la => la.Status == LAStatus.Draft)
                    .ForEach(la => la.Status = LAStatus.PendingApproval);
            } 
            else
            {
                if (draft.DocumentLegislativeAreas.All(la => la.Status is LAStatus.Published or LAStatus.Declined or LAStatus.DeclinedByOpssAdmin or LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS or LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS or LAStatus.DeclinedToRemoveByOPSS or LAStatus.DeclinedToUnarchiveByOPSS))
                {
                    draft.SubStatus = SubStatus.None;
                }
            }

            if (draft.StatusValue == Status.Published)
            {
                draft.StatusValue = Status.Draft;
                Audit auditCreated = new(userAccount, AuditCABActions.Created);
                draft.AuditLog = new List<Audit> { auditCreated };
                draft.CreatedByUserGroup = userAccount.Role!.ToLower();
                draft = await _cabRepository.CreateAsync(draft, auditCreated.DateTime);
            }
            else if (draft.StatusValue == Status.Draft)
            {
                await _cabRepository.UpdateAsync(draft);
            }

            await UpdateSearchIndexAsync(draft);
            await RefreshCachesAsync(draft.CABId, draft.URLSlug);
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
                await UpdateSearchIndexAsync(previous);
            }

            // Update the search index & caches
            await _cachedSearchService.RemoveFromIndexAsync(draft.id);
            await RefreshCachesAsync(draft.CABId, draft.URLSlug);
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
            await UpdateSearchIndexAsync(document);
            await RefreshCachesAsync(document.CABId, document.URLSlug);
        }

        public async Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument,
            string? publishInternalReason = default, string? publishPublicReason = default)
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

            var cabId = new Guid(latestDocument.CABId);

            var docLaToArchive = latestDocument.DocumentLegislativeAreas
                .Where(docLa => docLa.Status is
                    LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or
                    LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin);

            foreach (var docLa in docLaToArchive)
            {
                await ArchiveLegislativeAreaAsync(userAccount, cabId, docLa.LegislativeAreaId);

                var scheduleIds = latestDocument.Schedules?.Where(f => 
                    f.LegislativeArea != null && 
                    f.LegislativeArea == docLa.LegislativeAreaName).Select(f => f.Id).ToList();

                if (scheduleIds != null)
                {
                    if (docLa.Status == LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin)
                    {
                        await ArchiveSchedulesAsync(userAccount, cabId, scheduleIds);
                    }
                    else if (docLa.Status == LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin)
                    {
                        await RemoveSchedulesAsync(userAccount, cabId, scheduleIds);
                    }
                }
            }

            var docLaToUnArchive = latestDocument.DocumentLegislativeAreas
                .Where(docLa => docLa.Status == LAStatus.ApprovedToUnarchiveByOPSS);

            foreach (var docLa in docLaToUnArchive)
            {
                await UnArchiveLegislativeAreaAsync(userAccount, cabId, docLa.LegislativeAreaId);
            }               

            if (latestDocument.CreatedByUserGroup == Roles.OPSS.Id)
            {
                latestDocument.DocumentLegislativeAreas.ForEach(la => la.Status = LAStatus.Published);
            }
            else
            {
                latestDocument.DocumentLegislativeAreas.Where(la => la.Status is 
                    LAStatus.ApprovedByOpssAdmin or
                    LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin or
                    LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin or
                    LAStatus.DeclinedToRemoveByOGD or 
                    LAStatus.DeclinedToRemoveByOPSS or
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD or
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS or
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD or
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS or
                    LAStatus.ApprovedToUnarchiveByOPSS
                ).ForEach(la => la.Status = LAStatus.Published);

                await RemoveLegislativeAreasNotApprovedByOPSS(latestDocument);
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

            await UpdateSearchIndexAsync(latestDocument);

            await RefreshCachesAsync(latestDocument.CABId, urlSlug);

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

            await RefreshCachesAsync(publishedVersion.CABId, publishedVersion.URLSlug);

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
            await UpdateSearchIndexAsync(archivedDoc);

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

            archivedDoc = await _cabRepository.CreateAsync(archivedDoc, DateTime.Now);
            await UpdateSearchIndexAsync(archivedDoc);

            await RefreshCachesAsync(archivedDoc.CABId, archivedDoc.URLSlug);

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

            await UpdateSearchIndexAsync(publishedVersion);

            await RefreshCachesAsync(publishedVersion.CABId, publishedVersion.URLSlug);

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

        public async Task<DocumentScopeOfAppointment> GetDocumentScopeOfAppointmentAsync(Guid cabId,
            Guid scopeOfAppointmentId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString());
            return latestDocument?.ScopeOfAppointments.First(s => s.Id == scopeOfAppointmentId) ??
                   throw new InvalidOperationException();
        }

        public async Task<DocumentLegislativeArea> GetDocumentLegislativeAreaByLaIdAsync(Guid cabId,
            Guid laId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("Document not found");
            return latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == laId) ??
                   throw new InvalidOperationException();
        }

        public async Task<Guid> AddLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid laToAdd,
            string laName, string roleId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            if (latestDocument.DocumentLegislativeAreas.Any(l => l.LegislativeAreaId == laToAdd))
                throw new ArgumentException("Legislative id already exists on cab");
            var guid = Guid.NewGuid();
            latestDocument.DocumentLegislativeAreas.Add(new DocumentLegislativeArea
            {
                Id = guid,
                LegislativeAreaName = laName,
                LegislativeAreaId = laToAdd,
                RoleId = roleId,
                Status = LAStatus.Draft,
            });

            await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
            return guid;
        }

        public async Task RemoveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId,
            string laName)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            // remove document legislative area
            var documentLegislativeArea =
                latestDocument?.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId) ??
                throw new InvalidOperationException("No legislative area found");
            latestDocument.DocumentLegislativeAreas.Remove(documentLegislativeArea);

            // remove scope of appointment
            var scopeOfAppointments = latestDocument.ScopeOfAppointments
                .Where(n => n.LegislativeAreaId == legislativeAreaId).ToList();
            List<string> blobsToBeDeleted = new();

            foreach (var scopeOfAppointment in scopeOfAppointments)
            {
                latestDocument.ScopeOfAppointments.Remove(scopeOfAppointment);
            }

            // remove product schedules     
            if (latestDocument.Schedules != null && latestDocument.Schedules.Any())
            {
                var schedules = latestDocument.Schedules
                    .Where(n => n.LegislativeArea != null && n.LegislativeArea == laName).ToList();

                foreach (var schedule in schedules)
                {
                    // check if same blob not used by any other schedule, delete it if not
                    if (latestDocument?.Schedules?.Where(n => n.Id != schedule.Id && n.BlobName == schedule.BlobName)
                            .Count() == 0)
                    {
                        blobsToBeDeleted.Add(schedule.BlobName);
                    }

                    latestDocument.Schedules.Remove(schedule);
                }
            }

            await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);

            if (blobsToBeDeleted.Any())
            {
                await DeleteBlobs(blobsToBeDeleted);
            }
        }

        public async Task ArchiveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            // archive document legislative area
            var documentLegislativeArea =
                latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);
            documentLegislativeArea.Archived = true;

            await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
        }

        public async Task UnArchiveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId, string? reason = default)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            var documentLegislativeArea =
                latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);

            documentLegislativeArea.Status = LAStatus.Draft;
            documentLegislativeArea.Archived = false;

            if(!String.IsNullOrWhiteSpace(reason))
            {
                documentLegislativeArea.RequestReason = reason;
            }

            var scheduleIds = latestDocument.Schedules?.Where(f =>
                  f.LegislativeArea != null &&
                  f.LegislativeArea == documentLegislativeArea.LegislativeAreaName).Select(f => f.Id).ToList();

            if (scheduleIds != null && scheduleIds.Any())
            {
                var selectedSchedules = latestDocument.Schedules?.Where(n => scheduleIds.Contains(n.Id)).ToList();

                if (selectedSchedules != null && selectedSchedules.Any())
                {
                    foreach (var schedule in selectedSchedules)
                    {
                        schedule.Archived = false;
                    }                    
                }
            }

            await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
        }

        public async Task ApproveLegislativeAreaAsync(UserAccount approver, Guid cabId, Guid legislativeAreaId, LAStatus approvedLAStatus)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            // Approve document legislative area
            var documentLegislativeArea =
                latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);
            documentLegislativeArea.Status = approvedLAStatus;

            documentLegislativeArea.Archived = documentLegislativeArea.Status switch
            {
                LAStatus.ApprovedToUnarchiveByOPSS => false,
                LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin => true,
                _ => documentLegislativeArea.Archived
            };
            latestDocument.AuditLog.Add(new Audit(approver, AuditCABActions.ApproveLegislativeArea));
            await UpdateOrCreateDraftDocumentAsync(approver, latestDocument);
        }

        public async Task DeclineLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId, string reason, LAStatus declinedLAStatus)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            // decline document legislative area
            var documentLegislativeArea =
                latestDocument.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == legislativeAreaId);
            documentLegislativeArea.Status = declinedLAStatus;
            reason = "Legislative area " + documentLegislativeArea.LegislativeAreaName + " declined: </br>" + reason;
            latestDocument.AuditLog.Add(new Audit(userAccount,AuditCABActions.DeclineLegislativeArea, reason));
            await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
        }

        public async Task ArchiveSchedulesAsync(UserAccount userAccount, Guid cabId, List<Guid> ScheduleIds)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            if (ScheduleIds != null && ScheduleIds.Any())
            {
                var selectedSchedules = latestDocument.Schedules?.Where(n => ScheduleIds.Contains(n.Id)).ToList();

                if (selectedSchedules != null && selectedSchedules.Any())
                {
                    foreach (var schedule in selectedSchedules)
                    {
                        schedule.Archived = true;
                    }

                    await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                }
            }
        }       

        public async Task RemoveSchedulesAsync(UserAccount userAccount, Guid cabId, List<Guid> ScheduleIds)
        {
            var latestDocument = await GetLatestDocumentAsync(cabId.ToString()) ??
                                 throw new InvalidOperationException("No document found");

            if (ScheduleIds != null && ScheduleIds.Any())
            {
                var selectedSchedules = latestDocument.Schedules?.Where(n => ScheduleIds.Contains(n.Id)).ToList();

                if (selectedSchedules != null && selectedSchedules.Any())
                {
                    List<string> blobsToBeDeleted = new();

                    foreach (var schedule in selectedSchedules)
                    {
                        // check if same blob not used by any other schedule, delete it if not
                        if (latestDocument?.Schedules
                                ?.Where(n => n.Id != schedule.Id && n.BlobName == schedule.BlobName).Count() == 0)
                        {
                            blobsToBeDeleted.Add(schedule.BlobName);
                        }

                        latestDocument?.Schedules?.Remove(schedule);
                    }

                    await UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);

                    if (blobsToBeDeleted.Any())
                    {
                        await DeleteBlobs(blobsToBeDeleted);
                    }
                }
            }
        }

        public async Task<List<Document>> FindAllDocumentsByCABIdAsync(string id)
        {
            List<Document> docs = await _cabRepository.Query<Document>(d =>
                d.CABId.Equals(id, StringComparison.CurrentCultureIgnoreCase));

            return docs.OrderByDescending(d => d.LastUpdatedDate)
                .ToList();
        }

        public Document? GetLatestDocumentFromDocuments(List<Document> documents)
        {
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

        public async Task<bool> IsSingleDraftDocAsync(Guid cabId)
        {
            var cabDocuments = await FindAllDocumentsByCABIdAsync(cabId.ToString());
            return cabDocuments.Count == 1 && cabDocuments.First().StatusValue == Status.Draft;
        }

        private async Task RefreshCachesAsync(string cabId, string slug)
        {
            await _cachedSearchService.ClearAsync();
            await _cachedSearchService.ClearAsync(cabId);
            await _cachedPublishedCabService.ClearAsync(cabId, slug);
        }

        private async Task DeleteBlobs(List<string> blobNamesList)
        {
            foreach (var blobName in blobNamesList)
            {
                await _fileStorage.DeleteCABSchedule(blobName);
            }
        }

        public async Task RemoveLegislativeAreasToApprovedToRemoveByOPSS(Document document)
        {
            var documentLAList = document.DocumentLegislativeAreas.Where(la => la.Status == LAStatus.ApprovedToRemoveByOpssAdmin).ToList();

            foreach (DocumentLegislativeArea documentLegislativeArea in documentLAList)
            {
                // remove scope of appointment
                var scopeOfAppointments = document.ScopeOfAppointments
                    .Where(n => n.LegislativeAreaId == documentLegislativeArea.LegislativeAreaId).ToList();
                List<string> blobsToBeDeleted = new();

                foreach (var scopeOfAppointment in scopeOfAppointments)
                {
                    document.ScopeOfAppointments.Remove(scopeOfAppointment);
                }

                // remove product schedules     
                if (document.Schedules != null && document.Schedules.Any())
                {
                    var schedules = document.Schedules
                        .Where(n => n.LegislativeArea != null && n.LegislativeArea == documentLegislativeArea.LegislativeAreaName).ToList();

                    foreach (var schedule in schedules)
                    {
                        // check if same blob not used by any other schedule, delete it if not
                        // only remove blobs for LAs approved to remove by OPSS Admin
                        if (documentLegislativeArea.Status == LAStatus.ApprovedToRemoveByOpssAdmin &&
                            document?.Schedules?.Where(n => n.Id != schedule.Id && n.BlobName == schedule.BlobName).Count() == 0)
                        {
                            blobsToBeDeleted.Add(schedule.BlobName);
                        }

                        document.Schedules.Remove(schedule);
                    }
                }

                if (blobsToBeDeleted.Any())
                {
                    await DeleteBlobs(blobsToBeDeleted);
                }

                document.DocumentLegislativeAreas.Remove(documentLegislativeArea);
            }

        }

        private async Task RemoveLegislativeAreasNotApprovedByOPSS(Document document)
        {
            var documentLAList = document.DocumentLegislativeAreas.Where(la => la.Status != LAStatus.Published).ToList();

            foreach (DocumentLegislativeArea documentLegislativeArea in documentLAList)
            {
                // remove scope of appointment
                var scopeOfAppointments = document.ScopeOfAppointments
                    .Where(n => n.LegislativeAreaId == documentLegislativeArea.LegislativeAreaId).ToList();
                List<string> blobsToBeDeleted = new();

                foreach (var scopeOfAppointment in scopeOfAppointments)
                {
                    document.ScopeOfAppointments.Remove(scopeOfAppointment);
                }

                // remove product schedules     
                if (document.Schedules != null && document.Schedules.Any())
                {
                    var schedules = document.Schedules
                        .Where(n => n.LegislativeArea != null && n.LegislativeArea == documentLegislativeArea.LegislativeAreaName).ToList();

                    foreach (var schedule in schedules)
                    {
                        // check if same blob not used by any other schedule, delete it if not
                        // only remove blobs for LAs approved to remove by OPSS Admin
                        if (documentLegislativeArea.Status == LAStatus.ApprovedToRemoveByOpssAdmin && 
                            document?.Schedules?.Where(n => n.Id != schedule.Id && n.BlobName == schedule.BlobName).Count() == 0)
                        {
                            blobsToBeDeleted.Add(schedule.BlobName);
                        }

                        document.Schedules.Remove(schedule);
                    }
                }

                if (blobsToBeDeleted.Any())
                {
                    await DeleteBlobs(blobsToBeDeleted);
                }

                document.DocumentLegislativeAreas.Remove(documentLegislativeArea);
            }  
            
        }
    }
}