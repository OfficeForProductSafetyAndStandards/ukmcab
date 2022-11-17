using Microsoft.Extensions.Caching.Memory;

namespace UKMCAB.Common;

public static class Memory
{
    private static readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    public static void Set<T>(string scope, string key, T value) => _memoryCache.Set(Create(scope, key), value);
    public static T? Get<T>(string scope, string key) => (T?)(_memoryCache.Get(Create(scope, key)) ?? default(T));
    private static string Create(string scope, string key) => $"{scope}_{key}";
}
