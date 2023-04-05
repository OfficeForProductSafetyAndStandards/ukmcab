using Microsoft.Azure.Cosmos;
using System.Runtime.CompilerServices;

namespace UKMCAB.Data;

public static class ExtensionMethods
{
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> query)
    {
        var it = Microsoft.Azure.Cosmos.Linq.CosmosLinqExtensions.ToFeedIterator<T>(query);
        var asyncEnumerable = it.AsAsyncEnumerable();
        return asyncEnumerable;
    }

    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this FeedIterator<T> iterator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken);
            foreach (var item in page)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }
        }
    }
}