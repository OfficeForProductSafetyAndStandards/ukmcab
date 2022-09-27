using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
