using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

public class HomeController : Controller
{
    [Route("/")]
    [Route("home")]
    public IActionResult Index()
    {
        return View();
    }
}
