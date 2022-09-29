namespace UKMCAB.Web.UI.Models.ViewModels;

public class SearchResultsViewModel
{
    public string Keywords { get; set; }
    public string[] BodyTypes { get; set; }
    public string[] RegisteredOfficeLocations { get; set; }
    public string[] TestingLocations { get; set; }
    public string[] Regulations { get; set; }


    public FilterOptionsViewModel BodyTypeOptions { get; set; }
    public FilterOptionsViewModel RegisteredOfficeLocationOptions { get; set; }
    public FilterOptionsViewModel TestingLocationOptions { get; set; }
    public FilterOptionsViewModel RegulationOptions { get; set; }

    public List<SearchResultViewModel> SearchResultViewModels { get; set; }

    public void CheckSelecetedItems()
    {
        if (BodyTypes != null && BodyTypes.Any())
        {
            foreach (var bodyType in BodyTypes)
            {
                BodyTypeOptions.Options.Single(bt => bt.Value.Equals(bodyType, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (RegisteredOfficeLocations != null && RegisteredOfficeLocations.Any())
        {
            foreach (var registeredOfficeLocation in RegisteredOfficeLocations)
            {
                RegisteredOfficeLocationOptions.Options.Single(rol => rol.Value.Equals(registeredOfficeLocation, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (TestingLocations != null && TestingLocations.Any())
        {
            foreach (var testingLocation in TestingLocations)
            {
                TestingLocationOptions.Options.Single(tl => tl.Value.Equals(testingLocation, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
        if (Regulations != null && Regulations.Any())
        {
            foreach (var regulation in Regulations)
            {
                RegulationOptions.Options.Single(la => la.Value.Equals(regulation, StringComparison.InvariantCultureIgnoreCase)).Checked = true;
            }
        }
    }
}