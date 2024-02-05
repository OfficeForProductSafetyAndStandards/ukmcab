using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;


[Area("admin"), Route("admin/legislative-area"), Authorize]
public class LegislativeAreaAdditionalInformationController : Controller
{
    public static class Routes
    {
        public const string LegislativeAreaAdditionalInformation = "legislative.area.additional.information";

    }

    public LegislativeAreaAdditionalInformationController()
    {

    }
    
    //TODO: work in-progress
    [HttpGet("additional-information", Name = Routes.LegislativeAreaAdditionalInformation)]
    public async Task<IActionResult> DetailsAsync()
    {
        var vm = new LegislativeAreaAdditionalInformationViewModel(Title: "Legislative area: additional information");

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
    }

    //TODO: work in-progress
    [HttpPost("additional-information", Name = Routes.LegislativeAreaAdditionalInformation)]
    public async Task<IActionResult> DetailsAsync(LegislativeAreaAdditionalInformationViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

        return null;
    }
}
