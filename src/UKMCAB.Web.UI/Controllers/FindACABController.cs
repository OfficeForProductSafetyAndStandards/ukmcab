using Microsoft.AspNetCore.Mvc;
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
        return View();
    }
    
    [Route("find-a-cab/results")]
    public IActionResult Results(SearchResultsViewModel searchResultsViewModel)
    {
        LoadFilters(searchResultsViewModel);

        searchResultsViewModel.SearchResultViewModels = _cabSearchService.Search(searchResultsViewModel.Keywords,
            searchResultsViewModel.BodyTypes, 
            searchResultsViewModel.RegisteredOfficeLocations,
            searchResultsViewModel.TestingLocations, 
            searchResultsViewModel.Regulations);

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
    public IActionResult Profile(string id)
    {
        var cabProfile = _cabSearchService.GetCAB(id);
        return View(cabProfile);
    }
}