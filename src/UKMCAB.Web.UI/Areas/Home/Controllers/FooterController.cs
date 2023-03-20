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
        
        [Route("/accessibility-statement")]
        public IActionResult AccessibilityStatement()
        {
            return View();
        }        
        
        [Route("/terms-and-conditions")]
        public IActionResult TermsAndConditions()
        {
            return View();
        }
    }
}
