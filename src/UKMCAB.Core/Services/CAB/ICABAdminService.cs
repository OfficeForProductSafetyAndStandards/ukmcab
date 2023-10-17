using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

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
    }
}