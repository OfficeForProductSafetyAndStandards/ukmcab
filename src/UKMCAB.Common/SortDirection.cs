namespace UKMCAB.Common;

public static class SortDirectionHelper
{
    public const string Ascending = "asc";
    public const string Descending = "desc";
    public static string Get(string? value) => value == Descending ? Descending : Ascending;
    public static string GetFriendly(string? value) => value == Descending ? "descending" : "ascending";
    public static string Opposite(string? value) => Get(value) == Ascending ? Descending : Ascending;
}