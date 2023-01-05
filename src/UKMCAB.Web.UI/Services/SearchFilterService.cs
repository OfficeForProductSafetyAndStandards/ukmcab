namespace UKMCAB.Web.UI.Services;

public class SearchFilterService : ISearchFilterService
{
    public List<FilterOption> BodyTypeFilter { get; }
    public List<FilterOption> RegisteredOfficeLocationFilter { get; }
    public List<FilterOption> TestingLocationFilter { get; }
    public List<FilterOption> RegulationFilter { get; }


    public SearchFilterService()
    {
        BodyTypeFilter = CreateFilterOptions(Constants.Lists.BodyTypes, "ukmcab-bodytype"); 
        RegisteredOfficeLocationFilter = CreateFilterOptions(Constants.Lists.Countries, "ukmcab-registeredofficelocation"); 
        TestingLocationFilter = CreateFilterOptions(Constants.Lists.Countries, "ukmcab-testinglocation"); 
        RegulationFilter = CreateFilterOptions(Constants.Lists.LegislativeAreas, "ukmcab-regulation"); 
    }

    private static List<FilterOption> CreateFilterOptions(List<string> filters, string idPrefix)
    {
        return filters.Select(f => new FilterOption
        {
            Id = $"{idPrefix}-{SanitiseIdString(f)}" ,
            Value = f
        }).ToList();
    }

    private static string SanitiseIdString(string id)
    {
        return id
            .ToLower()
            .Replace(":", string.Empty)
            .Replace(" ", string.Empty);
    }
}