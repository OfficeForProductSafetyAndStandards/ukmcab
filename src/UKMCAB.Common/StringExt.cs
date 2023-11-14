using MoreLinq;

namespace UKMCAB.Common;

public static class StringExt
{
    /// <summary>
    /// Joins strings together - IGNORING empty/null/whitespace items.
    /// </summary>
    /// <param name="separator"></param>
    /// <param name="values"></param>
    /// <returns>The concatenated string or null.  Will never return empty/whitespace.</returns>
    public static string? Join(string separator, params object?[]? values)
        => values != null && values.Any() ? string.Join(separator, values.Flatten().Select(x => x?.ToString()).Where(x => x.IsNotNullOrEmpty())).Clean() : null;

    public static string Keyify(params object?[]? values) => Join("-", values) ?? string.Empty;
}