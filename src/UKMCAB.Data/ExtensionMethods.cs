using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using UKMCAB.Common;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

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

    public static string? GetAddress(this Document? cab) => StringExt.Join(", ", cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country);
    public static string? GetFormattedAddress(this Document cab) => StringExt.Join("<br />", new[] { cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country}.Where(x => !string.IsNullOrWhiteSpace(x)));

    public static string Expression(this SortBy sortBy, string defaultName) => $"{sortBy.Name ?? defaultName} {SortDirectionHelper.Get(sortBy.Direction)}";
}