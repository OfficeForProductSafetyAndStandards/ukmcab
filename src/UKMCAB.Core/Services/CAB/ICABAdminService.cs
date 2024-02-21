using UKMCAB.Core.Domain.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Services.CAB
{
    public interface ICABAdminService
    {
        //todo Change these methods to use CabModel as return value / params instead of Document
        Task<List<CabModel>> FindOtherDocumentsByCabNumberOrUkasReference(string cabId, string? cabNumber,
            string? ukasReference);

        Task<bool> DocumentWithSameNameExistsAsync(Document document);
        
        Task<List<Document>> FindAllDocumentsByCABURLAsync(string id, Status[]? statusesToRetrieve = null);

        Task<List<Document>> FindAllDocumentsByCABIdAsync(string id);

        /// <summary>
        /// Find all Draft and Archived documents restricted by user role
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns>If null userRole returns all documents</returns>
        Task<List<CabModel>> FindAllCABManagementQueueDocumentsForUserRole(string userRole);

        Task<Document?> GetLatestDocumentAsync(string cabId);

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
            string? publishInternalReason = default(string), string? publishPublicReason = default(string));

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

        Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string CABId, string? unarchiveInternalReason,
            string unarchivePublicReason, bool requestedByUkas);

        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
        Task<int> GetCABCountForStatusAsync(Status status = Status.Unknown);
        Task<int> GetCABCountForSubStatusAsync(SubStatus subStatus = SubStatus.None);

        Task<DocumentScopeOfAppointment> GetDocumentScopeOfAppointmentAsync(Guid cabId, Guid scopeOfAppointmentId);
        Task<DocumentLegislativeArea> GetDocumentLegislativeAreaByLaIdAsync(Guid cabId, Guid laId);

        /// <summary>
        /// Adds a Legislative area and sets the labels for search
        /// </summary>
        /// <param name="cabId">cab to update</param>
        /// <param name="laToAdd">New Legislative area id to create</param>
        /// <param name="laName">Name of Legislative Area to add to labels</param>
        /// <returns>DocumentLegislativeId created</returns>
        Task<Guid> AddLegislativeAreaAsync(Guid cabId, Guid laToAdd, string laName);

        Task RemoveLegislativeAreaAsync(Guid cabId, Guid legislativeAreaId, string laName);

        Task ArchiveLegislativeAreaAsync(Guid cabId, Guid legislativeAreaId);        

        Task ArchiveSchedulesAsync(Guid cabId, List<Guid> ScheduleIds);
    }
}