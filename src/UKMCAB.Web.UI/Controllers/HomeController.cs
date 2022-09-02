using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

[Route("")]
public class HomeController : Controller
{
    public class Routes
    {
        public const string Index = "home.index";
        public const string Search = "home.search";
        public const string SearchResults = "home.search-results";
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
    public IActionResult SearchResults()
    {
        return View();
    }
}
