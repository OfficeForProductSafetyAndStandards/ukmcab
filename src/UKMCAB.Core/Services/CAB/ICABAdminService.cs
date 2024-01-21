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
        Task<List<CabModel>> FindDocumentsByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABURLAsync(string id, Status[]? statusToRetrieve = null);
        /// <summary>
        /// Find all Draft and Archived documents restricted by user role
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns>If null userRole returns all documents</returns>
        Task<List<CabModel>> FindAllCABManagementQueueDocumentsForUserRole(string userRole);
        Task<Document?> GetLatestDocumentAsync(string cabId);
        Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document, bool saveAsDraft = false);

        Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft,
            bool submitForApproval = false);

        Task DeleteDraftDocumentAsync(UserAccount userAccount, Guid cabId, string? deleteReason);
        Task SetSubStatusAsync(Guid cabId, Status status, SubStatus subStatus, Audit audit);
        Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument, string? publishInternalReason = default(string), string? publishPublicReason = default(string));
        Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string? archiveInternalReason, string archivePublicReason);
        
        /// <summary>
        /// Un-publishes a CAB. Status becomes Historical and the CAB is not visible to users.
        /// </summary>
        /// <param name="userAccount">User un-publishing</param>
        /// <param name="cabId">Cab to unpublish</param>
        /// <param name="archiveInternalReason">Reason for unpublish</param>
        /// <returns></returns>
        Task UnPublishDocumentAsync(UserAccount userAccount, string cabId, string? archiveInternalReason);
        Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string CABId, string? unarchiveInternalReason, string unarchivePublicReason, bool requestedByUkas);
        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
        Task<int> GetCABCountForStatusAsync(Status status = Status.Unknown);
        Task<int> GetCABCountForSubStatusAsync(SubStatus subStatus = SubStatus.None);
    }
}