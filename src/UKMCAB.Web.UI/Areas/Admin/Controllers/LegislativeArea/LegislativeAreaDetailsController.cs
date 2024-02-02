using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;


[Area("admin"), Route("admin/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        
    }
    public LegislativeAreaDetailsController()
    {
         
    }
    
    [HttpGet("details", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details()
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }
}