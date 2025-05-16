using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace UKMCAB.Core.Tests.TestAsyncHelpers;

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression)
        => new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => new TestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression)
        => _inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
    {
        // If the result is a Task<T>, unwrap and simulate it
        if (typeof(TResult).Name.StartsWith("Task"))
        {
            var resultType = typeof(TResult).GetGenericArguments().FirstOrDefault();
            var result = _inner.Execute(expression);

            var taskResult = typeof(Task)
                .GetMethod(nameof(Task.FromResult))
                ?.MakeGenericMethod(resultType)
                .Invoke(null, new[] { result });

            return (TResult)taskResult!;
        }

        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        => Execute<TResult>(expression); // delegate to the fixed Execute
}
