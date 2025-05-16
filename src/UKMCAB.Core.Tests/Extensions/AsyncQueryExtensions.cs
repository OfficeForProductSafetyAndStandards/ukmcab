using System.Collections.Generic;
using System.Linq;
using UKMCAB.Core.Tests.TestAsyncHelpers;

namespace UKMCAB.Core.Tests.Extensions;

public static class AsyncQueryExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}