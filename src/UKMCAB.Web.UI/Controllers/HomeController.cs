using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly ISearchFilterService _searchFilterService;
    private ICABSearchService _cabSearchService;

    public HomeController(ISearchFilterService searchFilterService, ICABSearchService cabSearchService)
    {
        _cabSearchService = cabSearchService;
        _searchFilterService = searchFilterService;
    }
    
    public class Routes
    {
        public const string Index = "home.index";
        public const string Search = "home.search";
        public const string SearchResults = "home.search-results";
        public const string Profile = "home.profile";
    }

    [HttpGet("", Name = Routes.Index)]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("search", Name = Routes.Search)]
    public IActionResult Search()
    {
        return View();
    }

    [HttpGet("results", Name = Routes.SearchResults)]
    public IActionResult SearchResults(SearchResultsViewModel searchResultsViewModel)
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
        searchResultsViewModel.BodyTypeOptions = _searchFilterService.BodyTypeFilter;
        searchResultsViewModel.RegisteredOfficeLocationOptions = _searchFilterService.RegisteredOfficeLocationFilter;
        searchResultsViewModel.TestingLocationOptions = _searchFilterService.TestingLocationFilter;
        searchResultsViewModel.RegulationOptions = _searchFilterService.RegulationFilter;
        searchResultsViewModel.CheckSelecetedItems();
    }
    
    [HttpGet("profile", Name = Routes.Profile)]
    public IActionResult Profile(string id)
    {
        var cabProfile = _cabSearchService.GetCAB(id);
        return View(cabProfile);
    }
}
