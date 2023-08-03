using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Web.UI.Models.ViewModels;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        private readonly ISecureTokenProcessor _secureTokenProcessor;

        public static class Routes
        {
            public const string Message = "home.message";
        }

        public HomeController(ISecureTokenProcessor secureTokenProcessor)
        {
            _secureTokenProcessor = secureTokenProcessor;
        }

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

        [Route("/updates")]
        public IActionResult Updates()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.Updates
            };
            return View(model);
        }

        [HttpGet("~/m", Name = Routes.Message)]
        public IActionResult Message(string token)
        {
            var model = _secureTokenProcessor.Disclose<MessageViewModel>(token) ?? throw new DomainException("The incoming token deserialised to null");
            return View(model.ViewName ?? "Panel", model);
        }
    }
}