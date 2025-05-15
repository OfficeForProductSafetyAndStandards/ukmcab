using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Data;

public static class QueryablePagingExtensions
{
    public static async IAsyncEnumerable<Page<T>> AsPages<T>(
        this Task<IQueryable<T>> source,
        int? pageSize,
        string? continuationToken = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int defaultPageSize = 50;
        int size = pageSize ?? defaultPageSize;

        var query = await source.ConfigureAwait(false);
        int skip = 0;

        // Parse the continuation token (if provided)
        if (!string.IsNullOrEmpty(continuationToken) && int.TryParse(continuationToken, out var parsedSkip))
        {
            skip = parsedSkip;
        }

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        while (skip < totalCount)
        {
            var items = await query
                .Skip(skip)
                .Take(size)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            skip += items.Count;
            var nextToken = skip < totalCount ? skip.ToString() : null;

            yield return Page<T>.FromValues(items, nextToken);
        }
    }
}

