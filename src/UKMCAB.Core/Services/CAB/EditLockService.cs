using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Services.CAB;

public class EditLockService : IEditLockService
{
    private const string EditLockCacheKey = "CabEditLock";
    private readonly IDistCache _distCache;
    private readonly Dictionary<string, string> _items;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

    public EditLockService(IDistCache distCache)
    {
        _distCache = distCache;
        _items = _distCache.Get<Dictionary<string, string>?>(EditLockCacheKey) ?? new Dictionary<string, string>();
    }

    ///<inheritdoc />
    public async Task<string?> LockExistsForCabAsync(string cabId)
    {
        var cabsWithEditLock = await _distCache.GetAsync<Dictionary<string, string>?>(EditLockCacheKey) ?? new();
        return cabsWithEditLock.GetValueOrDefault(cabId);
    }
   
    ///<inheritdoc />
    public async Task SetAsync(string cabId, string userId)
    {
        _items.Add(cabId, userId);
        await _distCache.SetAsync(EditLockCacheKey, _items, _cacheDuration);
    }
    
    ///<inheritdoc />
    public async Task RemoveEditLockForUserAsync(string userId)
    {
        var cabEditLockFound = _items.FirstOrDefault(i => i.Value.Equals(userId)).Key ?? null;
        if (cabEditLockFound != null)
        {
            _items.Remove(cabEditLockFound);
            await _distCache.SetAsync(EditLockCacheKey, _items);
        }
    }
}