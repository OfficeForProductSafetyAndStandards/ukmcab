using UKMCAB.Data;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Services
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

        Task<Document> CreateDocumentAsync(UKMCABUser user, Document document, bool saveAsDraft = false);
        Task<Document> UpdateOrCreateDraftDocumentAsync(UKMCABUser user, Document draft, bool saveAsDraft = false);
        Task<bool> DeleteDraftDocumentAsync(string cabId);
        Task<Document> PublishDocumentAsync(UKMCABUser user, Document latestDocument);
        Task<Document> ArchiveDocumentAsync(UKMCABUser user, Document latestDocument, string archiveReason);
        IAsyncEnumerable<string> GetAllCabIds();
        Task RecordStatsAsync();
    }
}
