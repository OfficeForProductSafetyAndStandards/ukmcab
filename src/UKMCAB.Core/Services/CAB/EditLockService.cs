using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Services.CAB;

public class EditLockService : IEditLockService
{
    private const string _editLockCacheKey = "CabEditLock";
    private readonly IDistCache _distCache;
    private readonly Dictionary<string, string> _items = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    public EditLockService(IDistCache distCache)
    {
        _distCache = distCache;
    }

    public async Task<string?> LockExistsForCabAsync(string cabId)
    {
        var cabsWithEditLock = await _distCache.GetAsync<Dictionary<string, string>?>(_editLockCacheKey) ?? new();
        return cabsWithEditLock.GetValueOrDefault(cabId);
    }

    public async Task SetAsync(string cabId, string userId)
    {
        _items.Add(cabId, userId);
        await _distCache.SetAsync(_editLockCacheKey, _items, _cacheDuration);
    }

    public async Task RemoveEditLockForUserAsync(string userId)
    {
        var cabEditLockFound = _items.FirstOrDefault(i => i.Value.Equals(userId)).Key ?? null;
        if (cabEditLockFound != null)
        {
            _items.Remove(cabEditLockFound);
            await _distCache.SetAsync(_editLockCacheKey, _items);
        }
    }
}