using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Services.CAB;

public class EditLockService : IEditLockService
{
    private const string EditLockCacheKey = "CabEditLock";
    private readonly IDistCache _distCache;
    private readonly Dictionary<string, string> _items;
    private readonly TimeSpan _cacheDuration =
        TimeSpan.FromMinutes(Common.ApplicationConstants.SessionTimeoutDuration);

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
        if (_items.TryAdd(cabId, userId))
        {
            await _distCache.SetAsync(EditLockCacheKey, _items, _cacheDuration);
        }
    }
    
    ///<inheritdoc />
    public async Task RemoveEditLockForUserAsync(string userId)
    {
        var cabEditLocksFound = _items.Where(i => i.Value.Equals(userId)).ToList();
        if (cabEditLocksFound.Any())
        {
            foreach (var keyValuePair in cabEditLocksFound)
            {
                _items.Remove(keyValuePair.Key);
            }
            await _distCache.SetAsync(EditLockCacheKey, _items);
        }
    }

    ///<inheritdoc />
    public async Task RemoveEditLockForCabAsync(string cabId)
    {
        var cabEditLockFound = _items.ContainsKey(cabId);
        if (cabEditLockFound)
        {
            _items.Remove(cabId);
            await _distCache.SetAsync(EditLockCacheKey, _items);
        }
    }
}