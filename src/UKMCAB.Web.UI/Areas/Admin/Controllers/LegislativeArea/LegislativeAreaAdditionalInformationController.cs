using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/"), Authorize]
public class LegislativeAreaAdditionalInformationController : Controller
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;

    private const string PageTitle = "{0}: additional information";
    private const string StoragePageTitle = "pageTitle";

    public static class Routes
    {
        public const string LegislativeAreaAdditionalInformation = "legislative.area.additional.information";
    }

    public LegislativeAreaAdditionalInformationController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
    }

    [HttpGet("additional-information/{laId}", Name = Routes.LegislativeAreaAdditionalInformation)]
    public async Task<IActionResult> AdditionalInformationAsync(Guid id, Guid laId, string? returnUrl)
    {
        var legislativeArea = await _cabAdminService.GetDocumentLegislativeAreaAsync(id, laId);
        var legislativeAreaService = await _legislativeAreaService.GetLegislativeAreaByIdAsync(
                Guid.Parse(legislativeArea.LegislativeAreaId.ToString() ?? string.Empty));
        TempData[StoragePageTitle] = string.Format(PageTitle, legislativeAreaService?.Name);
        TempData.Keep(StoragePageTitle);
        var vm = new LegislativeAreaAdditionalInformationViewModel(Title: TempData[StoragePageTitle]?.ToString())
        {
            CabId = id,
            LegislativeAreaId = laId,
            IsProvisionalLegislativeArea = legislativeArea.IsProvisional,
            AppointmentDate = legislativeArea.AppointmentDate,
            ReviewDate = legislativeArea.ReviewDate,
            Reason = legislativeArea.Reason
        };

        return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
    }

    [HttpPost("additional-information/{laId}", Name = Routes.LegislativeAreaAdditionalInformation)]
    public async Task<IActionResult> AdditionalInformationAsync(LegislativeAreaAdditionalInformationViewModel vm,
        Guid id, Guid laId, string submitType, string? returnUrl)
    {
        vm.Title = TempData.Peek(StoragePageTitle)?.ToString();
        vm.CabId = id;
        vm.LegislativeAreaId = laId;

        if (submitType == Constants.SubmitType.Add18)
        {
            vm.ReviewDate = DateTime.UtcNow.AddMonths(18);
            ModelState.Clear();
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

        if (vm.AppointmentDate != null)
        {
            DateUtils.CheckDate(ModelState, vm.AppointmentDate.Value.Day.ToString(),
                vm.AppointmentDate.Value.Month.ToString(),
                vm.AppointmentDate.Value.Year.ToString(), nameof(vm.AppointmentDate), "appointment date");
        }

        if (vm.ReviewDate != null)
        {
            DateUtils.CheckDate(ModelState, vm.ReviewDate.Value.Day.ToString(),
                vm.ReviewDate.Value.Month.ToString(),
                vm.ReviewDate.Value.Year.ToString(), nameof(vm.ReviewDate), "review date");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        latestDocument!.DocumentLegislativeAreas.First(a => a.Id == laId).IsProvisional =
            vm.IsProvisionalLegislativeArea;
        latestDocument!.DocumentLegislativeAreas.First(a => a.Id == laId).AppointmentDate =
            vm.AppointmentDate;
        latestDocument!.DocumentLegislativeAreas.First(a => a.Id == laId).ReviewDate =
            vm.ReviewDate;
        latestDocument!.DocumentLegislativeAreas.First(a => a.Id == laId).Reason =
            vm.Reason;

        var userAccount =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

        TempData.Remove(StoragePageTitle);
        return submitType switch
        {
            Constants.SubmitType.Continue => RedirectToRoute(
                LegislativeAreaReviewController.Routes.LegislativeAreaSelected, new { id }),
            _ => RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true })
        };
    }
}