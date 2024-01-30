using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Core.Services.CAB;

public class EditLockService : IEditLockService
{
    private const string EditLockCacheKey = "CabEditLock";
    private readonly IDistCache _distCache;
    private readonly Dictionary<string, Tuple<string, DateTime>> _items; // CabId, UserId, ExpirationTime

    private readonly TimeSpan _cacheDuration =
        TimeSpan.FromMinutes(Common.ApplicationConstants.SessionTimeoutDuration);

    public EditLockService(IDistCache distCache)
    {
        _distCache = distCache;
        _items = _distCache.Get<Dictionary<string, Tuple<string, DateTime>>?>(EditLockCacheKey) ??
                 new Dictionary<string, Tuple<string, DateTime>>();
    }

    ///<inheritdoc />
    public async Task<string?> LockExistsForCabAsync(string cabId)
    {
        var cabsWithEditLock =
            await _distCache.GetAsync<Dictionary<string, Tuple<string, DateTime>>?>(EditLockCacheKey) ?? new();
        var cacheItem = cabsWithEditLock.GetValueOrDefault(cabId);
        if (cacheItem == null)
            return null;
        var lockExists = cacheItem.Item2 > DateTime.Now;
        return lockExists ? cacheItem.Item1 : null;
    }

    ///<inheritdoc />
    public async Task SetAsync(string cabId, string userId)
    {
        _items[cabId] = new Tuple<string, DateTime>(userId,
            DateTime.Now.AddMinutes(Common.ApplicationConstants.SessionTimeoutDuration));
        await _distCache.SetAsync(EditLockCacheKey, _items, _cacheDuration);
    }

    ///<inheritdoc />
    public async Task RemoveEditLockForUserAsync(string userId)
    {
        var cabEditLocksFound = _items.Where(i => i.Value.Item1.Equals(userId)).ToList();
        if (cabEditLocksFound.Any())
        {
            foreach (var keyValuePair in cabEditLocksFound)
            {
                _items.Remove(keyValuePair.Key);
            }

            await _distCache.SetAsync(EditLockCacheKey, _items, _cacheDuration);
        }
    }

    ///<inheritdoc />
    public async Task RemoveEditLockForCabAsync(string cabId)
    {
        var cabEditLockFound = _items.ContainsKey(cabId);
        if (cabEditLockFound)
        {
            _items.Remove(cabId);
            await _distCache.SetAsync(EditLockCacheKey, _items, _cacheDuration);
        }
    }
}