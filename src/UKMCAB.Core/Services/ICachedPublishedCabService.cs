using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Services;
public interface ICachedPublishedCabService
{
    Task<Document> FindPublishedDocumentByCABIdAsync(string id);
    Task<int> PreCacheAllCabsAsync();
    Task ClearAsync(string id);

public class CachedPublishedCabService : ICachedPublishedCabService
{
    private readonly ICABAdminService _cabs;
    private readonly IDistCache _cache;

    public CachedPublishedCabService(ICABAdminService cabAdminService, IDistCache cache)
    {
        _cabs = cabAdminService;
        _cache = cache;
    }

    public async Task<Document> FindPublishedDocumentByCABIdAsync(string id) => await _cache.GetOrCreateAsync(Key(id), () => _cabs.FindPublishedDocumentByCABIdAsync(id));

    public async Task ClearAsync(string id) => await _cache.RemoveAsync(Key(id));

    private static string Key(string id) => $"cab_{id}";

    public async Task<int> PreCacheAllCabsAsync()
    {
        var count = 0;
        var ids = _cabs.GetAllCabIds();
        await foreach(var id in ids)
        {
            _ = await FindPublishedDocumentByCABIdAsync(id);
            count++;
        }
        return count;
    }
}