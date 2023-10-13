using MoreLinq;
using System.Text.Json;
using UKMCAB.Common;
using UKMCAB.Data.Search.Models;
using UKMCAB.Infrastructure.Cache;

namespace UKMCAB.Data.Search.Services;

internal class CachedSearchService : ICachedSearchService
{
    private readonly IDistCache _cache;
    private readonly ISearchService _search;

    private const string _globalSearchesCacheKey = $"__{DataConstants.Version.Number}_searches";
    private const string _searchCacheKeyPrefix = $"{DataConstants.Version.Number}_srch_";
    private const string _cabSearchResultCacheKeyPrefix = $"{DataConstants.Version.Number}_cab_res_";
    private const string _facetsCacheKey = $"{DataConstants.Version.Number}_ukmcab-facets";
    private const string _facetsInternalCacheKey = $"{_facetsCacheKey}-internal";

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
        await _cache.RemoveAsync(_facetsInternalCacheKey);
        await _cache.RemoveAsync(_facetsCacheKey);
    }

    public async Task<SearchFacets> GetFacetsAsync(bool internalSearch) 
        => await _cache.GetOrCreateAsync(internalSearch ? _facetsInternalCacheKey : _facetsCacheKey, () => _search.GetFacetsAsync(internalSearch), TimeSpan.FromHours(1));

    public async Task<CABResults> QueryAsync(CABSearchOptions options)
    {
        if (options.Keywords?.Contains("~noc") ?? false)
        {
            options.Keywords = options.Keywords.Replace("~noc", string.Empty).Trim();
            return await _search.QueryAsync(options);
        }
        else
        {
            var k = $"{_searchCacheKeyPrefix}{JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = false }).Md5()}";
            var rv = await _cache.GetOrCreateAsync(k, () => _search.QueryAsync(options), TimeSpan.FromHours(5), async result =>
            {
                var ids = result.CABs.Select(x => x.CABId).ToList();
                var tasks = ids.Select(x => _cache.SetAddAsync(GetCabSearchResultSetCacheKey(x), k));
                await Task.WhenAll(tasks);

                await _cache.SetAddAsync(_globalSearchesCacheKey, k);
            });
            return rv;
        }
    }

    private static string GetCabSearchResultSetCacheKey(string cabId) => $"{_cabSearchResultCacheKeyPrefix}{cabId}";

    public async Task ReIndexAsync(CABIndexItem cabIndexItem)
    {
        await _search.ReIndexAsync(cabIndexItem);
    }

    public async Task RemoveFromIndexAsync(string id)
    {
        await _search.RemoveFromIndexAsync(id);
    }
}
