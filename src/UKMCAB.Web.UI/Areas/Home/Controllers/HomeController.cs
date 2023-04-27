using Microsoft.AspNetCore.Mvc;
using UKMCAB.Web.UI.Models.ViewModels.Home;

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

        [Route("/help")]
        public IActionResult Help()
        {
            return View();
        }

        [Route("/contact-us")]
        public IActionResult ContactUs()
        {
            return View(new ContactUsViewModel());
        }

        [HttpPost]
        [Route("/contact-us")]
        public IActionResult ContactUs(ContactUsViewModel model)
        {
            return View(model);
        }

    }
}
