using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin.Notification;
using UKMCAB.Web.UI.Models.ViewModels.Shared;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Route("admin/notifications"), Authorize]
public class NotificationController : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string sf, string sd, int pageNumber = 1)
    {
        var items = new List<(string From, string Subject, string CABName, string SentOn, string CABLink)>
        {
            new("From test", "Subject test", "CAB name test", DateTime.Now.ToShortDateString(), "view cab link")
        };
        //todo connect to service
        var model = new NotificationsViewModel
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

    [HttpGet("details")]
    public IActionResult Detail(string id)
    {
        //todo connect to service
        var statuses = new List<(int Value, string Text)>
        {
            (1, "Unassigned"),
            (2, "Assigned"),
            (3, "Completed"),
        };
        var assignees = new List<(string Value, string Text)>
        {
            ("user1", "Test User 1"),
            ("user2", "Test User 2")
        };
        var vm = new NotificationDetailViewModel(
            "Notification Details",
            "Notification: Test Notification",
            "1",
            statuses,
            "From value", "Subject value", "reason value", "11/10/2023 12:15", "12/10/2023 13:00", ("view cab", "/"),
            "Mr BPSS", "12/10/2023 11:00", assignees, assignees.First().Value, "BPSS"
        );
        return View(vm);
    }
}