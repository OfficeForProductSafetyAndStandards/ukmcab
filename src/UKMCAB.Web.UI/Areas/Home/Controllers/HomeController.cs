namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        [Route("/about")]
        public IActionResult About()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.About
            };
            return View(model);
        }

        [Route("/help")]
        public IActionResult Help()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.Help
            };
            return View(model);
        }
    }
}