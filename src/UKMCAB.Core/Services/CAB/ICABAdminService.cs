using UKMCAB.Core.Domain;
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
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABURLAsync(string id);
        /// <summary>
        /// Find all Draft and Archived documents restricted by user role
        /// </summary>
        /// <param name="userRole"></param>
        /// <returns>If null userRole returns all documents</returns>
        Task<List<Document>> FindAllCABManagementQueueDocumentsForUserRole(String userRole);
        Task<Document?> GetLatestDocumentAsync(string id);
        Task<Document> CreateDocumentAsync(UserAccount userAccount, Document document, bool saveAsDraft = false);

        Task<Document> UpdateOrCreateDraftDocumentAsync(UserAccount userAccount, Document draft,
            bool submitForApproval = false);

        Task<Document> PublishDocumentAsync(UserAccount userAccount, Document latestDocument);
        Task<Document> ArchiveDocumentAsync(UserAccount userAccount, string CABId, string archiveInternalReason, string archivePublicReason);
        Task<Document> UnarchiveDocumentAsync(UserAccount userAccount, string CABId, string unarchiveInternalReason, string unarchivePublicReason);
        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
        Task<int> GetCABCountForStatusAsync(Status status = Status.Unknown);
        Task<int> GetCABCountForSubStatusAsync(SubStatus subStatus = SubStatus.None);
    }
}