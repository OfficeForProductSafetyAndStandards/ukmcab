namespace UKMCAB.Core.Domain;

public record GeoAddress(string? AddressLine1, string AddressLine2, string? TownCity, string County, string? PostCode, string? Country);