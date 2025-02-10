using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq.Expressions;
using UKMCAB.Data.CosmosDb.Utilities;
using UKMCAB.Data.Pagination;

namespace UKMCAB.Data.CosmosDb.Services;

public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : class
{
    private CosmosClient _cosmosClient;
    private ICosmosFeedIterator _cosmosFeedIterator;
    private Container _container;

    /// <summary>
    /// Read-only repository for any CosmosDb container.
    /// ContainerId parameter is set in each dependency injection definition in program.cs.
    /// </summary>
    /// <param name="cosmosClient">Cosmos client.</param>
    /// <param name="cosmosFeedIterator">A wrapper around the CosmosLinqQuery.ToFeedIterator() method to enable unit testing.</param>
    /// <param name="containerId">Cosmos container name, e.g. "legislative-areas".</param>
    public ReadOnlyRepository(CosmosClient cosmosClient, ICosmosFeedIterator cosmosFeedIterator, string containerId)
    {
        _cosmosClient = cosmosClient;
        _cosmosFeedIterator = cosmosFeedIterator;
        _container = _cosmosClient.GetContainer(DataConstants.CosmosDb.Database, containerId);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var feedIterator = _container.GetItemQueryIterator<T>();

        var list = new List<T>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            list.AddRange(response.Resource.Select(r => r));
        }

        return list;
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate)
    {
        var query = _container.GetItemLinqQueryable<T>().Where(predicate);
        var feedIterator = _cosmosFeedIterator.GetFeedIterator<T>(query);
        
        var list = new List<T>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            list.AddRange(response.Resource.Select(r => r));
        }

        return list;
    }

    public async Task<(IEnumerable<O> Results, PaginationInfo PaginationInfo)> PaginatedQueryAsync<O>(
        Expression<Func<O, bool>> predicate, 
        int pageNumber, 
        string? searchTerm = null, 
        int pageSize = 20) where O : IOrderable
    {
        var query = _container.GetItemLinqQueryable<O>().Where(predicate);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || x.ReferenceNumber.Any(refNum => refNum.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }
        var queryCount = await query.CountAsync();

        var paginationInfo = new PaginationInfo(pageNumber, queryCount);
        query = query.OrderBy(x => x.Name).Skip(paginationInfo.Skip).Take(paginationInfo.Take);

        var feedIterator = _cosmosFeedIterator.GetFeedIterator(query);

        var list = new List<O>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            list.AddRange(response.Resource.Select(r => r));
        }

        return (list, paginationInfo);
    }

    public async Task<T> GetAsync(string id)
    {
        return await _container.ReadItemAsync<T>(id, new PartitionKey(id));
    }
}
