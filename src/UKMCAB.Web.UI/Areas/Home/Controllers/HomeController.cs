using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        [Route("/about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("/faq")]
        public IActionResult FAQ()
        {
            return View();
        }
    }
}
