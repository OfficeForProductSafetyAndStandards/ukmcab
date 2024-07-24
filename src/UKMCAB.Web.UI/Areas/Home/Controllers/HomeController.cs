using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core.Extensions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Areas.Search.Controllers;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Models.ViewModels.Home;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        private readonly ISecureTokenProcessor _secureTokenProcessor;
        private readonly IEditLockService _editLockService;

        public HomeController(ISecureTokenProcessor secureTokenProcessor, IEditLockService editLockService)
        {
            _secureTokenProcessor = secureTokenProcessor;
            _editLockService = editLockService;
        }

        [Route("/about")]
        public async Task<IActionResult> About()
        {
            await _editLockService.RemoveEditLockForUserAsync(User.GetUserId()!);
            var model = new BasicPageModel
            {
                Title = Constants.PageTitle.About
            };
            return View(model);
        }

        [Route("/help")]
        public async Task<IActionResult> Help()
        {
            await _editLockService.RemoveEditLockForUserAsync(User.GetUserId()!);
            var model = new BasicPageModel
            {
                Title = Constants.PageTitle.Help
            };
            return View(model);
        }

        [HttpGet("~/m", Name = Routes.Message)]
        public IActionResult Message(string token)
        {
            var model = _secureTokenProcessor.Disclose<MessageViewModel>(token) ?? throw new DomainException("The incoming token deserialised to null");
            return View(model.ViewName ?? "Panel", model);
        }

        [Route("/updates", Name = Routes.Updates)]
        public async Task<IActionResult> Updates()
        {
            await _editLockService.RemoveEditLockForUserAsync(User.GetUserId()!);
            var model = new UpdatesViewModel
            {
                FeedLinksViewModel = new FeedLinksViewModel()
            };

            ShareUtils.AddDetails(HttpContext, model.FeedLinksViewModel);

            return View(model);
        }

        [Route("search-feed")]
        public IActionResult SearchFeed()
        {
            //Permanent redirect for existing feed subscribers
            return RedirectPermanent(Url.RouteUrl(SearchController.Routes.SearchFeed) ??
                              throw new InvalidOperationException());
        }

        public static class Routes
        {
            public const string Message = "home.message";
            public const string Updates = "home.update";
        }
    }
}