namespace UKMCAB.Infrastructure.Cache;

public static class CacheKey
{
    public static string Make(string name, string subname, params object[] items) => $"{name}.{subname}({string.Join(',', items.Select(x => x?.ToString()))})";
}
