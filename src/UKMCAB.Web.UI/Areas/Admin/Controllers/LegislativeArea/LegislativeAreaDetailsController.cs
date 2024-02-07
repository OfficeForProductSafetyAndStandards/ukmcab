using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly IEditLockService _editLockService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        public const string AddLegislativeArea = "legislative.area.add-legislativearea";
        public const string AddPurposeOfAppointment = "legislative.area.add-purpose-of-appointment";
        public const string AddCategory = "legislative.area.add-category";
        public const string AddSubCategory = "legislative.area.add-sub-category";
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

    [HttpGet("details/{laId}", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details(Guid id, Guid laId)
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");

        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }

    [HttpGet("add", Name = Routes.AddLegislativeArea)]
    public async Task<IActionResult> AddLegislativeArea(Guid id, string? returnUrl)
    {
        var legislativeareas = await _legislativeAreaService.GetAllLegislativeAreasAsync();

        var vm = new LegislativeAreaViewModel
        {
            CABId = id,
            LegislativeAreas = await this.GetLegislativeSelectListItemsAsync(),
            ReturnUrl = returnUrl,
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddLegislativeArea.cshtml", vm);
    }


    [HttpGet("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, Guid scopeId)
    {
        //todo get scope of appointment
        var laId = Guid.Parse("e840c3d0-6153-4baa-82ab-0374b81d46fe");
        var options = await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(laId);
        var legislativeArea = await _legislativeAreaService.GetLegislativeAreaByIdAsync(laId);
        if (legislativeArea == null)
        {
            throw new InvalidOperationException($"Legislative Area not found for {laId}");
        }

        if (!options.PurposeOfAppointments.Any())
        {
            return RedirectToRoute(Routes.AddCategory, new { id, scopeId });
        }

        var selectListItems = options.PurposeOfAppointments
            .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
        var vm = new PurposeOfAppointmentViewModel
        {
            Title = "Select purpose of appointment",
            LegislativeArea = legislativeArea.Name,
            PurposeOfAppointments = selectListItems,
            CabId = id
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
    }

    [HttpPost("add-purpose-of-appointment/{scopeId}", Name = Routes.AddPurposeOfAppointment)]
    public async Task<IActionResult> AddPurposeOfAppointment(Guid id, PurposeOfAppointmentViewModel vm, Guid scopeId)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }
        var documentScopeOfAppointment = latestDocument.ScopeOfAppointments.First(s => s.Id == scopeId);

        if (ModelState.IsValid)
        {
            documentScopeOfAppointment.PurposeOfAppointmentId = vm.SelectedPurposeOfAppointmentId;
            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

            return RedirectToRoute(Routes.AddCategory, new { id, scopeId });
        }
        else
        {
            var options =
                await _legislativeAreaService.GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(
                    documentScopeOfAppointment.LegislativeAreaId);
            vm.PurposeOfAppointments = options.PurposeOfAppointments
                .Select(poa => new SelectListItem(poa.Name, poa.Id.ToString())).ToList();
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AddPurposeOfAppointment.cshtml", vm);
        }
    }

    private async Task<IEnumerable<SelectListItem>> GetLegislativeSelectListItemsAsync()
    {
        var legislativeAreas = await _legislativeAreaService.GetAllLegislativeAreasAsync();
        return legislativeAreas.Select(x => new SelectListItem() { Text = x.Name, Value = x.Id.ToString() });
    }
}