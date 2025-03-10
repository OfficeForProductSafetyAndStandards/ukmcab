﻿using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public interface ISearchService
    {
        Task<SearchFacets> GetFacetsAsync(bool internalSearch);
        Task<CABResults> QueryAsync(CABSearchOptions options);

        Task ReIndexAsync(CABIndexItem doc);
        Task RemoveFromIndexAsync(string cabId);
    }
}
