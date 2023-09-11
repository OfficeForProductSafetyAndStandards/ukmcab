using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data.CosmosDb.Services
{
    public class CachedPublishedCABService : ICachedPublishedCABService
    {
        private readonly IDistCache _cache;
        private readonly ICABRepository _cabRepository;
        private const string KeyPrefix = $"{DataConstants.Version.Number}_cab_";

        public CachedPublishedCABService(IDistCache cache, ICABRepository cabRepository)
        {
            _cache = cache;
            _cabRepository = cabRepository;
        }
        public async Task<Document> FindPublishedDocumentByCABURLAsync(string url) => await _cache.GetOrCreateAsync(Key(url), () => GetPublishedCABByURLAsync(url));
        private async Task<Document?> GetPublishedCABByURLAsync(string url)
        {
            // Is url a guid
            if (Guid.TryParse(url, out var result))
            {
                var document = await GetPublishedCABByIdAsync(url);
                if (document != null)
                {
                    return document;
                }
            }
            // Is url for a published or archived CAB 
            var documents = await _cabRepository.Query<Document>(d => (d.StatusValue == Status.Published || d.StatusValue == Status.Archived) && d.URLSlug.Equals(url));
            if (documents != null && documents.Any() && documents.Count == 1)
            {
                return documents.First();
            }

            // Is url somewhere in a historical CAB which was last updated in the last two months
            documents = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Historical && d.URLSlug.Equals(url));
            if (documents != null && documents.Any() && documents.Count == 1 && documents.First().LastUpdatedDate > DateTime.UtcNow.AddMonths(-2))
            {
                var document = documents.First();
                // Find published version
                documents = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Published && d.CABId.Equals(document.CABId));
                if (documents != null && documents.Any() && documents.Count == 1 )
                {
                    return documents.First();
                }
            }

            return null;
        }

        public async Task<Document> FindPublishedDocumentByCABIdAsync(string id) => await _cache.GetOrCreateAsync(Key(id), () => GetPublishedCABByIdAsync(id));

        private async Task<Document> GetPublishedCABByIdAsync(string id)
        {
            var doc = await _cabRepository.Query<Document>(d => (d.StatusValue == Status.Published || d.StatusValue == Status.Archived) && d.CABId.Equals(id));
            return doc.Any() ? doc.OrderByDescending(d => d.LastUpdatedDate).First() : null;
        }

        private static string Key(string id) => $"{KeyPrefix}{id}";

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
