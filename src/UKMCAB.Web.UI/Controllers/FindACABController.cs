using Microsoft.AspNetCore.Mvc;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Controllers;

public class FindACABController : Controller
{
    private readonly ISearchFilterService _searchFilterService;
    private readonly ICABSearchService _cabSearchService;

    public FindACABController(ISearchFilterService searchFilterService, ICABSearchService cabSearchService)
    {
        _searchFilterService = searchFilterService;
        _cabSearchService = cabSearchService;
    }
    
    [Route("find-a-cab")]
    public IActionResult Index()
    {
        return View(new SearchViewModel());
    }

    [HttpPost]
    [Route("find-a-cab")]
    public IActionResult Index(SearchViewModel searchViewModel)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Results", new { Keywords = searchViewModel.Keywords });
        }
        return View();
    }


    [Route("find-a-cab/results")]
    public async Task<IActionResult> Results(SearchResultsViewModel searchResultsViewModel)
    {
        LoadFilters(searchResultsViewModel);

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
        return View(cabProfile);
    }
}