using System.ComponentModel;

namespace UKMCAB.Subscriptions.Core.Data.Models;

public abstract class Page<T>
{
    public abstract IReadOnlyList<T> Values { get; }

    public abstract string? ContinuationToken { get; }

    public static Page<T> FromValues(IReadOnlyList<T> values, string? continuationToken)
    {
        return new PageCore(values, continuationToken);
    }

    public override string? ToString() => base.ToString();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj) => base.Equals(obj);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => base.GetHashCode();

    private class PageCore : Page<T>
    {
        public PageCore(IReadOnlyList<T> values, string? continuationToken)
        {
            Values = values;
            ContinuationToken = continuationToken;
        }

        public override IReadOnlyList<T> Values { get; }
        public override string? ContinuationToken { get; }
    }
}

