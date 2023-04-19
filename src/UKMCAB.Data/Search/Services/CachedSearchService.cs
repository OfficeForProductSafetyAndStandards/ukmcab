using MoreLinq;
using System.Text.Json;
using UKMCAB.Common;
using UKMCAB.Data.Search.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data.Search.Services;

public interface ICachedSearchService : ISearchService 
{
    /// <summary>
    /// Clears down all search result cache items that contain a particular CAB.
    /// </summary>
    /// <param name="cabId"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This is to be used where a CAB is updated and you cannot know which CAB search result cache items contain a given CAB.
    /// </remarks>
    Task ClearAsync(string cabId);
    
    /// <summary>
    /// Clears all cached searches.  Useful for when a CAB is published/unpublished.
    /// Remember the CAB will need to have been index first, so may need to manually update the search index, before clearing cache items.
    /// </summary>
    /// <returns></returns>
    Task ClearAsync();
}

internal class CachedSearchService : ICachedSearchService
{
    private readonly IDistCache _cache;
    private readonly ISearchService _search;

    private const string _globalSearchesCacheKey = "__searches";

    public CachedSearchService(IDistCache cache, ISearchService search) 
    {
        _cache = cache;
        _search = search;
    }

    /// <inheritdoc />
    public async Task ClearAsync(string cabId)
    {
        var k = GetCabSearchResultSetCacheKey(cabId);
        string[] keys = (await _cache.GetSetMembersAsync(k));

        var tasks = keys.Select(key => _cache.RemoveAsync(key));
        await Task.WhenAll(tasks);

        await _cache.RemoveAsync(k);
    }

    /// <inheritdoc />
    public async Task ClearAsync()
    {
        string[] keys = (await _cache.GetSetMembersAsync(_globalSearchesCacheKey));
        var batches = keys.Batch(50); // clear N cache keys in parallel at a time (don't want to run an unlimited amount of tasks in parallel)
        foreach (var batch in batches)
        {
            var tasks = batch.Select(key => _cache.RemoveAsync(key));
            await Task.WhenAll(tasks);
        }
        await _cache.RemoveAsync(_globalSearchesCacheKey);
    }

    public async Task<SearchFacets> GetFacetsAsync() 
        => await _cache.GetOrCreateAsync("ukmcab-facets", _search.GetFacetsAsync, TimeSpan.FromHours(1));

    public async Task<CABResults> QueryAsync(CABSearchOptions options)
    {
        var k = $"srch_{JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = false }).Md5()}";
        var rv = await _cache.GetOrCreateAsync(k, () => _search.QueryAsync(options), TimeSpan.FromHours(5), async result =>
        {
            var ids = result.CABs.Select(x => x.CABId).ToList();
            var tasks = ids.Select(x => _cache.SetAddAsync(GetCabSearchResultSetCacheKey(x), k));
            await Task.WhenAll(tasks);

            await _cache.SetAddAsync(_globalSearchesCacheKey, k);
        });
        return rv;
    }

    private static string GetCabSearchResultSetCacheKey(string cabId) => $"cab_res_{cabId}";

    public async Task ReIndexAsync()
    {
        await _search.ReIndexAsync();
        await ClearAsync();
    }
}
