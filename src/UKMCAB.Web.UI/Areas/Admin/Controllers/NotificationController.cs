using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin"), Authorize]
public class NotificationController : Controller
{
    [HttpGet("notifications")]
    public async Task<IActionResult> Index(string sf, string sd, int pageNumber = 1)
    {
        var items = new List<(string From, string Subject, string CABName, string SentOn, string CABLink)>
        {
            new("From test", "Subject test", "CAB name test", DateTime.Now.ToShortDateString(), "view cab link")
        };
        //todo connect to service
        var model = new NotificationTableViewModel
        (
            Constants.PageTitle.Notifications,
            items.Any(),
            sf,
            SortDirectionHelper.Get(sd),
            items,
            new PaginationViewModel
            {
                PageNumber = pageNumber,
                ResultsPerPage = 5,
                Total = items.Count
            },
            new MobileSortTableViewModel(
                "asc",
                "sf",
                new List<Tuple<string, string>>()
                {
                    new("From", "From")
                }));
        return View(model);
    }
}