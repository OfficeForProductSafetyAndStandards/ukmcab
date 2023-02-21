using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public interface ISearchService
    {
        Task<CABResults> QueryAsync(CABSearchOptions options);
    }
}
