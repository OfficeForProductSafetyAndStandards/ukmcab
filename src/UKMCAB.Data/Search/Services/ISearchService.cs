using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public interface ISearchService
    {
        Task<FacetResult> GetFacetsAsync();
        Task<CABResults> QueryAsync(CABSearchOptions options);
    }
}
