using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public interface ISearchService
    {
        Task<SearchFacets> GetFacetsAsync();
        Task<CABResults> QueryAsync(CABSearchOptions options);

        /// <summary>
        /// This requested the indexer re-indexes the data source 
        /// AND waits for it to complete.  Could take 1-5 seconds to complete for 200 records.
        /// Should the system ever have 1000s of records we'll need another approach.
        /// </summary>
        /// <returns></returns>
        Task ReIndexAsync();
        Task RemoveFromIndexAsync(string cabId);
    }
}
