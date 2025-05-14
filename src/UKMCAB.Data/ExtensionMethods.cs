using UKMCAB.Common;
using UKMCAB.Data.Domain;
using UKMCAB.Data.Models;

namespace UKMCAB.Data;

public static class ExtensionMethods
{
    public static string? GetAddress(this Document? cab) => StringExt.Join(", ", cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country);
    public static string? GetFormattedAddress(this Document cab) => StringExt.Join("<br />", new[] { cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country}.Where(x => !string.IsNullOrWhiteSpace(x)));
    public static IEnumerable<string> GetAddressArray(this Document? cab) => (new string[] { cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country }).Where(x => !string.IsNullOrWhiteSpace(x)); 

    public static string Expression(this SortBy sortBy, string defaultName) => $"{sortBy.Name ?? defaultName} {SortDirectionHelper.Get(sortBy.Direction)}";
}