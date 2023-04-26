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
            var ids = _cabRepository.GetItemLinqQueryable().Select(x => x.CABId).AsAsyncEnumerable();
            await foreach (var id in ids)
            {
                _ = await FindPublishedDocumentByCABIdAsync(id);
                count++;
            }
            return count;
        }

        public async Task ClearAsync(string id) => await _cache.RemoveAsync(Key(id));
    }
}
