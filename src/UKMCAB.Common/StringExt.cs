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

    /// <summary>
    /// Truncate the provided string to the maximum provided length (lengh includes essipsis if added)
    /// NOTE, if your text ends in a . or space, those will be removed as well to avoid:
    ///  - "Some value ..."
    ///  - "Some value...."
    /// </summary>
    /// <param name="value">The input string</param>
    /// <param name="maxLength">Maximum length of output string</param>
    /// <returns></returns>
    public static string TruncateWithEllipsis(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || maxLength <= 0)
        {
            return string.Empty;
        }

        const string ellipsis = "...";
        if (value.Length <= maxLength)
        {
            return value;
        }

        // Adjust maxLength to account for the ellipsis
        int truncatedLength = maxLength - ellipsis.Length;
        if (truncatedLength <= 0)
        {
            return ellipsis;
        }

        return value.Substring(0, truncatedLength).TrimEnd('.').TrimEnd(' ') + ellipsis;
    }
}