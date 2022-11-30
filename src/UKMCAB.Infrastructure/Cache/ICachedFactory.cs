namespace UKMCAB.Infrastructure.Cache;

public interface ICachedFactory
{
    T GetOrCreate<T>(string key, Func<T> action, TimeSpan? expiry = null, Action<string> onCacheItemCreation = null, int databaseId = -1);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> action, TimeSpan? expiry = null, Func<string, Task> onCacheItemCreation = null, int databaseId = -1);
}
