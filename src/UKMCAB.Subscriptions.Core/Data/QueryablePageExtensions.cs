using System.Linq.Expressions;

namespace UKMCAB.Subscriptions.Core.Data;

public static class QueryablePageExtensions
{
    public static Task<IQueryable<T>> QueryAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>>? predicate = null,
        int? maxPerPage = null)
    {
        if (predicate != null)
        {
            source = source.Where(predicate);
        }

        if (maxPerPage.HasValue)
        {
            source = source.Take(maxPerPage.Value);
        }

        return Task.FromResult(source);
    }
}

