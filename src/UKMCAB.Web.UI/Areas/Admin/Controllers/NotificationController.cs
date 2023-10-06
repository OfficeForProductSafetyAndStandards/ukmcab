using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

//todo add authorize
[Area("admin"), Route("admin")]
public class NotificationController : Controller
{
    public NotificationController()
    {
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> Index(string sortField, string sortDirection)
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
            "sortField",
            SortDirectionHelper.Ascending,
            items,
            new PaginationViewModel()
            {
                PageNumber = 1,
                ResultsPerPage = 5,
                Total = items.Count
            }
        );
        return View(model);
    }
}