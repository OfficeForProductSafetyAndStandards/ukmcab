using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Controllers;

public class FindACABController : Controller
{
    private readonly ISearchFilterService _searchFilterService;
    private readonly ICABSearchService _cabSearchService;
    private readonly IAdminService _adminService;

    public FindACABController(ISearchFilterService searchFilterService, ICABSearchService cabSearchService, IAdminService adminService)
    {
        _searchFilterService = searchFilterService;
        _cabSearchService = cabSearchService;
        _adminService = adminService;
    }
    
    [Route("find-a-cab")]
    public async Task<IActionResult> Index()
    {
        var isAdmin = await _adminService.IsInRoleAsync(User, Constants.Roles.AuthRoles);
        return View(new SearchViewModel{IsAdmin = isAdmin});
    }

    [HttpPost]
    [Route("find-a-cab")]
    public async Task<IActionResult> Index(SearchViewModel searchViewModel)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Results", new { Keywords = searchViewModel.Keywords });
        }
        searchViewModel.IsAdmin = await _adminService.IsInRoleAsync(User, Constants.Roles.AuthRoles);

        return View(searchViewModel);
    }


    [Route("find-a-cab/results")]
    public async Task<IActionResult> Results(SearchResultsViewModel searchResultsViewModel)
    {
        LoadFilters(searchResultsViewModel);
        searchResultsViewModel.IsAdmin = await _adminService.IsInRoleAsync(User, Constants.Roles.AuthRoles);

        var cabs = await _cabSearchService.SearchCABsAsync(searchResultsViewModel.Keywords, new FilterSelections
        {
            BodyTypes = searchResultsViewModel.BodyTypes,
            RegisteredOfficeLocations = searchResultsViewModel.RegisteredOfficeLocations,
            TestingLocations = searchResultsViewModel.TestingLocations,
            Regulations = searchResultsViewModel.Regulations
        });

        searchResultsViewModel.SearchResultViewModels = cabs == null? new List<SearchResultViewModel>() : cabs.Select(c => new SearchResultViewModel
        {
            Address = c.Address,
            Email = c.Email,
            Name = c.Name,
            Phone = c.Phone,
            Website = c.Website,
            id = c.Id,
            Regulations = string.Join(", ", c.Regulations.Select(r => r.Name))
        }).ToList();

        return View(searchResultsViewModel);
    }
    
    private void LoadFilters(SearchResultsViewModel searchResultsViewModel)
    {
        searchResultsViewModel.BodyTypeOptions = new FilterOptionsViewModel
        {
            Id = "BodyTypes",
            Label = "Body type",
            Options = _searchFilterService.BodyTypeFilter
        };
        searchResultsViewModel.RegisteredOfficeLocationOptions = new FilterOptionsViewModel
        {
            Id = "RegisteredOfficeLocations",
            Label = "Registered office location",
            Options = _searchFilterService.RegisteredOfficeLocationFilter

        };
        searchResultsViewModel.TestingLocationOptions = new FilterOptionsViewModel
        {
            Id = "TestingLocations",
            Label = "Testing locations",
            Options = _searchFilterService.TestingLocationFilter
        };
        searchResultsViewModel.RegulationOptions = new FilterOptionsViewModel
        {
            Id = "Regulations",
            Label = "Regulations",
            Options = _searchFilterService.RegulationFilter
        };
        searchResultsViewModel.CheckSelecetedItems();
    }
    
    [Route("find-a-cab/profile")]
    public async Task<IActionResult> Profile(string id)
    {
        var cabProfile = await _cabSearchService.GetCABAsync(id);
        cabProfile.IsAdmin = await _adminService.IsInRoleAsync(User, Constants.Roles.AuthRoles);
        return View(cabProfile);
    }
}