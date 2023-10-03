using Microsoft.AspNetCore.Authorization;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

//todo add authorize
[Area("admin"), Route("admin")]
public class NotificationController : Controller
{
    
    public NotificationController()
    {
        
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> Index()
    {
        var model = new BasicPageModel
        {
            Title = Constants.PageTitle.About
        };
        return View(model);
    }
}