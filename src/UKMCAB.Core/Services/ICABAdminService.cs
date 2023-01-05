using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABAdminService
    {
        Task<Document> CreateCABDocumentAsync(string email, CABData cabData);

        Task<List<Document>> FindCABDocumentsAsync(string cabName);
    }
}
