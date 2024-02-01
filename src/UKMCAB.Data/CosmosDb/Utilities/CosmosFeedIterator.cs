namespace UKMCAB.Data.CosmosDb.Utilities
{
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;
    using System.Linq;

    /// <summary>
    /// This is a wrapper so that the call to the CosmosLinqQuery.ToFeedIterator method can be mocked in unit tests. The method throws 
    /// an error if the query is not an actual CosmosLinqQuery, and these cannot be created. The only workaround is to wrap that call 
    /// in a service which can be mocked.
    /// 
    /// Old code:
    /// var query = container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();
    /// 
    /// New code:
    /// var query = _container.GetItemLinqQueryable<T>().Where(predicate);
    /// var feedIterator = _cosmosFeedIterator.GetFeedIterator<T>(query);
    /// </summary>
    public class CosmosFeedIterator : ICosmosFeedIterator
    {
        public FeedIterator<T> GetFeedIterator<T>(IQueryable<T> query)
        {
            return query.ToFeedIterator();
        }
    }
}
