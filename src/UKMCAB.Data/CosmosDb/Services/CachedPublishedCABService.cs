using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CachedPublishedCABService : ICachedPublishedCABService
    {
        private readonly IDistCache _cache;
        private readonly ICABRepository _cabRepository;

        public CachedPublishedCABService(IDistCache cache, ICABRepository cabRepository)
        {
            _cache = cache;
            _cabRepository = cabRepository;
        }
        public async Task<Document> FindPublishedDocumentByCABURLAsync(string url) => await _cache.GetOrCreateAsync(Key(url), () => GetPublishedCABByURLAsync(url));
        private async Task<Document> GetPublishedCABByURLAsync(string url)
        {
            var doc = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Published && d.URLSlug.Equals(url));
            if (doc == null)
            {
                doc = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Published && d.URLSlugRedirect.Equals(url));
            }
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        public async Task<Document> FindPublishedDocumentByCABIdAsync(string id) => await _cache.GetOrCreateAsync(Key(id), () => GetPublishedCABByIdAsync(id));

        private async Task<Document> GetPublishedCABByIdAsync(string id)
        {
            var doc = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Published && d.CABId.Equals(id));
            return doc.Any() && doc.Count == 1 ? doc.First() : null;
        }

        private static string Key(string id) => $"cab_{id}";

        public async Task<int> PreCacheAllCabsAsync()
        {
            var count = 0;
            var cabs = _cabRepository.GetItemLinqQueryable().Select(x => new {id= x.CABId, slug = x.URLSlug}).AsAsyncEnumerable();
            await foreach (var cab in cabs)
            {
                _ = await FindPublishedDocumentByCABIdAsync(cab.id); 
                _ = await FindPublishedDocumentByCABURLAsync(cab.slug);
                count++;
            }
            return count;
        }

        public async Task ClearAsync(string id, string slug)
        { 
            await _cache.RemoveAsync(Key(id));
            await _cache.RemoveAsync(Key(slug));
        } 
    }
}
