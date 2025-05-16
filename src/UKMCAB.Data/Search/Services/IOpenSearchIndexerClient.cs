using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    /// <summary>
    /// </summary>
    public interface IOpenSearchIndexerClient
    {
        Task <bool> IndexExistsAsync(string indexerName);
        Task CreateIndexAsync(string indexerName);
        Task DeleteIndexAsync(string indexerName);
        Task BulkIndexAsync(string indexerName, IEnumerable<CABIndexItem> documents);
        Task RunIndexerAsync(string indexerName, CancellationToken token = default);
    }
}
