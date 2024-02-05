using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using static UKMCAB.Web.UI.Constants;
using System.Linq;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;


[Area("admin"), Route("admin/cab/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IEditLockService _editLockService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        public const string LegislativeAreaAdd = "legislative.area.add-legislativearea";
    }
    public LegislativeAreaDetailsController(
        ICABAdminService cabAdminService,            
        IEditLockService editLockService,
        ILegislativeAreaService legislativeAreaService
    )
    {
        _cabAdminService = cabAdminService;        
        _editLockService = editLockService;
        _legislativeAreaService = legislativeAreaService;
    }
    
    [HttpGet("details/{id}", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details(Guid id)
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }

    [HttpGet("add/{id}", Name = Routes.LegislativeAreaAdd)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var legislativeareas = await _legislativeAreaService.GetAllLegislativeAreas();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await this.GetLegislativeSelectListItemsAsync(),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }

    [HttpPost("add/{id}", Name = Routes.LegislativeAreaAdd)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, LegislativeAreaViewModel vm, string submitType)
    {
        if (ModelState.IsValid)
        {
            // to do save legislative area/scope of appointment;

            if (submitType == Constants.SubmitType.Continue)
            {
                return RedirectToRoute(Routes.LegislativeAreaDetails, new { id });
            }
            // save additional info
            else if (submitType == Constants.SubmitType.AdditionalInfo)
            {
                return RedirectToRoute(Routes.LegislativeAreaDetails, new { id });
            }
            // save as draft
            else
            {
                return RedirectToAction("Summary", "CAB", new { Area = "admin", id, subSectionEditAllowed = true });
            }
        }
        else
        {
            vm.LegislativeAreas = await this.GetLegislativeSelectListItemsAsync();
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
        }
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync()
    {
        var legislativeareas = await _legislativeAreaService.GetAllLegislativeAreas();
        return legislativeareas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }
}