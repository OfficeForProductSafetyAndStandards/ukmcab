using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Helpers;
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
        // var appointmentDate = DateUtils.CheckDate(ModelState, vm.AppointmentDateDay , vm.AppointmentDateMonth,
        //     vm.AppointmentDateYear, nameof(vm.AppointmentDate), "appointment");
        
        // var appointmentDate = DateUtils.CheckDate(ModelState, vm.AppointmentDate.Value.Day.ToString() , vm.AppointmentDate.Value.Month.ToString(),
        //     vm.AppointmentDate.Value.Year.ToString(), nameof(vm.AppointmentDate), nameof(vm.AppointmentDate));

        // var reviewDate = DateUtils.CheckDate(ModelState, vm.ReviewDateDay, vm.ReviewDateMonth,
        //    vm.ReviewDateYear, nameof(vm.ReviewDate), nameof(vm.ReviewDate));
        
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

        return null;
    }
}
