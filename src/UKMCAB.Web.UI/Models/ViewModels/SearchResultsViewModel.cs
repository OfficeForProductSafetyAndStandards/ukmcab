namespace UKMCAB.Web.UI.Models.ViewModels;

public class SearchResultsViewModel
{
    public string Keywords { get; set; }
    public string[] BodyTypes { get; set; }
    public string[] RegisteredOfficeLocations { get; set; }
    public string[] TestingLocations { get; set; }
    public string[] LegislativeAreas { get; set; }
    public List<FilterOption> BodyTypeOptions { get; set; }
    public List<FilterOption> RegisteredOfficeLocationOptions { get; set; }
    public List<FilterOption> TestingLocationOptions { get; set; }
    public List<FilterOption> LegislativeAreaOptions { get; set; }
    public List<SearchResultViewModel> SearchResultViewModels { get; set; }

    public void CheckSelecetedItems()
    {
        if (BodyTypes != null && BodyTypes.Any())
        {
            foreach (var bodyType in BodyTypes)
            {
                BodyTypeOptions.Single(bt => bt.Value.Equals(bodyType, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (RegisteredOfficeLocations != null && RegisteredOfficeLocations.Any())
        {
            foreach (var registeredOfficeLocation in RegisteredOfficeLocations)
            {
                RegisteredOfficeLocationOptions.Single(rol => rol.Value.Equals(registeredOfficeLocation, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (TestingLocations != null && TestingLocations.Any())
        {
            foreach (var testingLocation in TestingLocations)
            {
                TestingLocationOptions.Single(tl => tl.Value.Equals(testingLocation, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (LegislativeAreas != null && LegislativeAreas.Any())
        {
            foreach (var legislativeArea in LegislativeAreas)
            {
                LegislativeAreaOptions.Single(la => la.Value.Equals(legislativeArea, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
    }
}