using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABAdminService
    {
        Task<bool> DocumentWithKeyIdentifiersExistsAsync(Document document);
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);
        Task<List<Document>> FindAllDocumentsByCABIdAsync(string id);
        Task<Document> CreateDocumentAsync(string userEmail, Document document);
        Task<bool> UpdateOrCreateDraftDocumentAsync(string email, Document draft);
        Task<bool> DeleteDraftDocumentAsync(string cabId);
    }
}
