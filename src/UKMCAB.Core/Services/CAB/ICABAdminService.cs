﻿using UKMCAB.Core.Domain;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Services.CAB
{
    public interface ICABAdminService
    {
        Task<List<Document>> FindOtherDocumentsByCabNumberOrUkasReference(string cabId, string? cabNumber,
            string? ukasReference);

        Task<bool> DocumentWithSameNameExistsAsync(Document document);
        
        Task<List<Document>> FindAllDocumentsByCABURLAsync(string id, Status[]? statusesToRetrieve = null);

        Task<List<Document>> FindAllDocumentsByCABIdAsync(string id);

        /// <summary>
        /// Find all CAB documents for the CAB Management screen, restricted by user role.
        /// </summary>
        /// <param name="userRole">RoleId of current user.</param>
        /// <returns></returns>
        Task<CabManagementDetailsModel> FindAllCABManagementQueueDocumentsForUserRole(string userRole);

        Task<Document?> GetLatestDocumentAsync(string cabId);

        /// <summary>
        /// Checks if the supplied CAB is pending approval by OGDs, and if so, removes any LA-related data
        /// for LAs that the current user is not linked to. Useful for screens where OGD users can only see
        /// the data for their own LA.
        /// </summary>
        /// <param name="latestDocument">The CAB document to filter.</param>
        /// <param name="userRoleId">RoleId of current user.</param>
        /// <returns></returns>
        Task FilterCabContentsByLaIfPendingOgdApproval(Document latestDocument, string userRoleId);

        /// <summary>
        /// Creates a new draft document with audit log Created
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document);

        /// <summary>
        /// Updates a document and clears the search index and cache.
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="draft">document with current status.
        /// If published creates a new draft
        /// If Draft updates draft with created audit log
        /// </param>
        /// <param name="submitForApproval">if true sub status becomes PendingApprovalToPublish</param>
        /// <returns>New document</returns>
        Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft,
            bool submitForApproval = false);

        Task DeleteDraftDocumentAsync(UserAccount userAccount, Guid cabId, string? deleteReason);
        Task SetSubStatusAsync(Guid cabId, Status status, SubStatus subStatus, Audit audit);

        Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument,
            string? publishInternalReason = default, string? publishPublicReason = default, string? publishType = default);

        Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string? archiveInternalReason,
            string archivePublicReason);

        /// <summary>
        /// Un-publishes a CAB. Status becomes Historical and the CAB is not visible to users.
        /// </summary>
        /// <param name="userAccount">User un-publishing</param>
        /// <param name="cabId">Cab to unpublish</param>
        /// <param name="archiveInternalReason">Reason for unpublish</param>
        /// <returns>Historical Document</returns>
        Task<Document> UnPublishDocumentAsync(UserAccount userAccount, string cabId, string? archiveInternalReason);

        /// <summary>
        /// Un-archives a CAB. Status becomes Draft, the CAB is not visible to public users, and only visible to Admin users.
        /// </summary>
        /// <param name="userAccount">User approving un-unarchiving the CAB.</param>
        /// <param name="cabId">Cab to unarchive</param>
        /// <param name="archiveInternalReason">Reason for unarchiving</param>
        /// <param name="legislativeAreasAsDraft">whether the Legislative Areas should be set as draft when un-archiving</param>
        /// <returns>Draft Document</returns>
        Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string CABId, string? unarchiveInternalReason,
            string unarchivePublicReason, bool requestedByUkas, bool legislativeAreasAsDraft = false);

        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
        Task<int> GetCABCountForStatusAsync(Status status = Status.Unknown);
        Task<int> GetCABCountForSubStatusAsync(SubStatus subStatus = SubStatus.None);

        Task<DocumentScopeOfAppointment> GetDocumentScopeOfAppointmentAsync(Guid cabId, Guid scopeOfAppointmentId);
        Task<DocumentLegislativeArea> GetDocumentLegislativeAreaByLaIdAsync(Guid cabId, Guid laId);

        /// <summary>
        /// Adds a Legislative area and sets the labels for search
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="cabId">cab to update</param>
        /// <param name="laToAdd">New Legislative area id to create</param>
        /// <param name="laName">Name of Legislative Area to add to labels</param>
        /// <param name="roleId"></param>
        /// <param name="MRABypass">Whether this LA is associated with MRA and has SOA bypassed</param>
        /// <returns>DocumentLegislativeId created</returns>
        Task<Guid> AddLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid laToAdd, string laName,
            string roleId, bool MRABypass);

        Task RemoveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId, string laName);

        Task ArchiveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId);

        Task UnArchiveLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId, string? reason, string? publicReason);

        Task ApproveLegislativeAreaAsync(UserAccount approver, Guid cabId, Guid legislativeAreaId, LAStatus approvedLAStatus);

        Task DeclineLegislativeAreaAsync(UserAccount userAccount, Guid cabId, Guid legislativeAreaId, string reason, LAStatus declinedLAStatus);     

        Task ArchiveSchedulesAsync(UserAccount userAccount, Guid cabId, List<Guid> ScheduleIds);       

        Task RemoveSchedulesAsync(UserAccount userAccount, Guid cabId, List<Guid> ScheduleIds);

        Document? GetLatestDocumentFromDocuments(List<Document> documents);

        Task<bool> IsSingleDraftDocAsync(Guid cabId);

        Task RemoveLegislativeAreasToApprovedToRemoveByOPSS(Document document);
    }
}