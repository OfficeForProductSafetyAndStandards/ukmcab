using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class FooterController : Controller
    {

        [Route("/privacy-notice")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/cookies-policy")]
        public IActionResult Cookies()
        {
            return View();
        }
    }
}
