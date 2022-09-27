using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

public class AboutController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}