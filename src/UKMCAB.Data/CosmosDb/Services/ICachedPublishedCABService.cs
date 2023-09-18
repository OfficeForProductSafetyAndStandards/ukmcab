using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services
{
    public interface ICachedPublishedCABService
    {
        Task<Document?> FindPublishedDocumentByCABURLAsync(string url);
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);

        Task<List<Document>> FindAllDocumentsByCABIdAsync(string cabId);

        Task<Document> FindDraftDocumentByCABIdAsync(string id);
        Task<int> PreCacheAllCabsAsync();
        Task ClearAsync(string id, string slug);
    }
}
