using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = $"{Constants.Roles.OGDUser}")]
    public class ReviewController : Controller
    {
        private readonly ICABAdminService _cabAdminService;

        public ReviewController(ICABAdminService cabAdminService)
        {
            _cabAdminService = cabAdminService;
        }

        [HttpGet]
        [Route("admin/review/list")]
        public async Task<IActionResult> List()
        {
            var model = new ReviewListViewModel
            {
                //SubmittedCABs = await _cabAdminService.FindCABDocumentsByStatesAsync(new[] { State.SubmittedForPublishing })
            };

            return View(model);
        }
    }
}
