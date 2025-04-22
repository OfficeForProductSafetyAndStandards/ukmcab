using UKMCAB.Common;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Interfaces.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data.CosmosDb.Services.CachedCAB
{
    public class CosmosCachedPublishedCABService : ICachedPublishedCABService
    {
        private readonly IDistCache _cache;
        private readonly ICABRepository _cabRepository;
        private const string KeyPrefix = $"{DataConstants.Version.Number}_cab_";

        public CosmosCachedPublishedCABService(IDistCache cache, ICABRepository cabRepository)
        {
            _cache = cache;
            _cabRepository = cabRepository;
        }
        
        /// <summary>
        /// Check if url is a guid and gets published cab by cab id or url slug
        /// Adds to cache if new or retrieves from cache if exists
        /// </summary>
        /// <param name="url">url or cab id!</param>
        /// <returns>Found document or null</returns>
        public async Task<Document> FindPublishedDocumentByCABURLOrGuidAsync(string url) => await _cache.GetOrCreateAsync(StringExt.Keyify(nameof(FindPublishedDocumentByCABURLOrGuidAsync), Key(url)), () => GetPublishedCABByURLOrGuidAsync(url));
        
        private async Task<Document?> GetPublishedCABByURLOrGuidAsync(string url)
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

        public async Task<List<Document?>> FindAllDocumentsByCABIdAsync(string cabId) => await _cache.GetOrCreateAsync(StringExt.Keyify(nameof(FindAllDocumentsByCABIdAsync), Key(cabId)), () =>  _cabRepository.Query<Document>(d => d.CABId.Equals(cabId)));

        public async Task<Document?> FindPublishedDocumentByCABIdAsync(string cabId) => await GetPublishedCABByIdAsync(cabId);

        public async Task<Document?> FindDraftDocumentByCABIdAsync(string cabId) => await  GetDraftCABByIdAsync(cabId);


        private async Task<Document?> GetPublishedCABByIdAsync(string cabId)
        {
            var docs = await FindAllDocumentsByCABIdAsync(cabId);
            return  docs.SingleOrDefault(d => d is { StatusValue: Status.Published or Status.Archived });
        }

        private async Task<Document?> GetDraftCABByIdAsync(string cabId)
        {
            var docs = await FindAllDocumentsByCABIdAsync(cabId);
            return docs.SingleOrDefault(d => d is { StatusValue: Status.Draft });
        }

        private static string Key(string id) => $"{KeyPrefix}{id}";

        public async Task<int> PreCacheAllCabsAsync()
        {
            var count = 0;
            var cabs = _cabRepository.GetItemLinqQueryable().Select(x => new {id= x.CABId, slug = x.URLSlug}).AsAsyncEnumerable();
            await foreach (var cab in cabs)
            {
                _ = await FindAllDocumentsByCABIdAsync(cab.id); 
                _ = await FindPublishedDocumentByCABURLOrGuidAsync(cab.slug);
                count++;
            }
            return count;
        }

        public async Task ClearAsync(string id, string slug)
        { 
            await _cache.RemoveAsync(Key(id));
            await _cache.RemoveAsync(Key(slug));
            await _cache.RemoveAsync(StringExt.Keyify(nameof(FindPublishedDocumentByCABURLOrGuidAsync), Key(slug)));
            await _cache.RemoveAsync(StringExt.Keyify(nameof(FindPublishedDocumentByCABURLOrGuidAsync), Key(id)));
            await _cache.RemoveAsync(StringExt.Keyify(nameof(FindAllDocumentsByCABIdAsync), Key(id)));
        } 
    }
}
