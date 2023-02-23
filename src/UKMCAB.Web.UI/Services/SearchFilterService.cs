namespace UKMCAB.Web.UI.Services;

public class SearchFilterService : ISearchFilterService
{
    public List<FilterOptionn> BodyTypeFilter { get; }
    public List<FilterOptionn> RegisteredOfficeLocationFilter { get; }
    public List<FilterOptionn> TestingLocationFilter { get; }
    public List<FilterOptionn> RegulationFilter { get; }


    public SearchFilterService()
    {
        BodyTypeFilter = CreateFilterOptions(Constants.Lists.BodyTypes, "ukmcab-bodytype"); 
        RegisteredOfficeLocationFilter = CreateFilterOptions(Constants.Lists.Countries, "ukmcab-registeredofficelocation"); 
        TestingLocationFilter = CreateFilterOptions(Constants.Lists.Countries, "ukmcab-testinglocation"); 
        RegulationFilter = CreateFilterOptions(Constants.Lists.LegislativeAreas, "ukmcab-regulation"); 
    }

    private static List<FilterOptionn> CreateFilterOptions(List<string> filters, string idPrefix)
    {
        return filters.Select(f => new FilterOptionn
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