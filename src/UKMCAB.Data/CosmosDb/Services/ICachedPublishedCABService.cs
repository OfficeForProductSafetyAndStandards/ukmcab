using UKMCAB.Data.Models;

namespace UKMCAB.Data.CosmosDb.Services
{
    public interface ICachedPublishedCABService
    {
        Task<Document> FindPublishedDocumentByCABIdAsync(string id);
        Task<int> PreCacheAllCabsAsync();
        Task ClearAsync(string id);
    }
}
