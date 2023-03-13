using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("home")]
        public IActionResult Index()
        {
            return View();
        }

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
