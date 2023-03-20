using UKMCAB.Core.Models;
using UKMCAB.Core.Models.Legacy;

namespace UKMCAB.Core.Services
{
    public interface ICABAdminService
    {
        Task<CABDocument> FindCABDocumentByIdAsync(string id);

        Task<Document> CreateCABDocumentAsync(string email, CABData cabData);
        Task<bool> UpdateCABAsync(string email, Document document);
        Task<List<Document>> FindCABDocumentsByNameAsync(string cabName);
        Task<List<Document>> FindCABDocumentsByIdAsync(string id);
        Task<List<Document>> FindCABDocumentsByUKASReferenceAsync(string ukasReference);
       //Task<List<Document>> FindCABDocumentsByStatesAsync(State[] states);

    }
}
