using UKMCAB.Core.Models;

namespace UKMCAB.Core.Services
{
    public interface ICABAdminService
    {
        Task<Document> CreateCABDocumentAsync(string email, CABData cabData, State state = State.Saved);

        Task<List<Document>> FindCABDocumentsByNameAsync(string cabName);
        Task<List<Document>> FindCABDocumentsByUKASReferenceAsync(string ukasReference);
    }
}
