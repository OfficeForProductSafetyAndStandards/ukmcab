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
        //todo connect to service
        var model = new NotificationTableViewModel
        (
            Constants.PageTitle.Notifications,
            true,
            sortField,
            SortDirectionHelper.Ascending,
            new List<(string From, string Subject, string CABName, string SentOn, string CABLink)>(),
            new PaginationViewModel()
        );
        return View(model);
    }
}