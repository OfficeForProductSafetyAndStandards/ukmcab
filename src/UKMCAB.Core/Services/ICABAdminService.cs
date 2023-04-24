using UKMCAB.Data.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABAdminService
    {
        Task<bool> DocumentWithKeyIdentifiersExistsAsync(Document document);
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABIdAsync(string id);
        Task<List<Document>> FindAllWorkQueueDocuments();
        Task<Document> GetLatestDocumentAsync(string id);

        Task<Document> CreateDocumentAsync(string userEmail, Document document);
        Task<Document> UpdateOrCreateDraftDocumentAsync(string email, Document draft, bool saveAsDraft = false);
        Task<bool> DeleteDraftDocumentAsync(string cabId);
        Task<Document> PublishDocumentAsync(string userEmail, Document latestDocument);
        IAsyncEnumerable<string> GetAllCabIds();
    }
}
