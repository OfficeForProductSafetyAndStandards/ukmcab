using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

public class AboutController : Controller
{
    [Route("about")]
    public IActionResult Index()
    {
        return View();
    }
}