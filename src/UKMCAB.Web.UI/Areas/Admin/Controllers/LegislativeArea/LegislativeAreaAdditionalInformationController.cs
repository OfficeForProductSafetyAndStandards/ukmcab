using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
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

    private const string UserNotesLabel = "User notes (optional)";
    private const string UserNotesLabelMandatory = "User notes (mandatory)";

    [HttpGet("additional-information/{laId}", Name = Routes.LegislativeAreaAdditionalInformation)]
    public async Task<IActionResult> AdditionalInformationAsync(Guid id, Guid laId, string? returnUrl, bool fromSummary)
    {
        var legislativeArea = await _cabAdminService.GetDocumentLegislativeAreaByLaIdAsync(id, laId);
        var legislativeAreaService = await _legislativeAreaService.GetLegislativeAreaByIdAsync(legislativeArea.LegislativeAreaId);
        TempData[StoragePageTitle] = string.Format(PageTitle, legislativeAreaService?.Name);
        TempData.Keep(StoragePageTitle);
        var vm = new LegislativeAreaAdditionalInformationViewModel(Title: TempData[StoragePageTitle]?.ToString())
        {
            CabId = id,
            LegislativeAreaId = laId,
            IsProvisionalLegislativeArea = legislativeArea.IsProvisional,
            AppointmentDate = legislativeArea.AppointmentDate,
            ReviewDate = legislativeArea.ReviewDate,
            UserNotesLabel = legislativeArea.ReviewDate.HasValue ? UserNotesLabelMandatory : UserNotesLabel,
            UserNotes = legislativeArea.UserNotes,
            Reason = legislativeArea.Reason,
            PointOfContactName = legislativeArea.PointOfContactName,
            PointOfContactEmail = legislativeArea.PointOfContactEmail,
            PointOfContactPhone = legislativeArea.PointOfContactPhone,
            IsPointOfContactPublicDisplay = legislativeArea.IsPointOfContactPublicDisplay,
            IsFromSummary = fromSummary,
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
        vm.UserNotesLabel = UserNotesLabel;

        if (submitType == Constants.SubmitType.Add18)
        {
            vm.ReviewDate = DateTime.UtcNow.AddMonths(18);
            vm.UserNotesLabel = UserNotesLabelMandatory;
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

        if ((!string.IsNullOrWhiteSpace(vm.PointOfContactName) ||
            !string.IsNullOrWhiteSpace(vm.PointOfContactEmail) ||
            !string.IsNullOrWhiteSpace(vm.PointOfContactPhone)) &&
                !vm.IsPointOfContactPublicDisplay.HasValue)
        {
            ModelState.AddModelError("IsPointOfContactPublicDisplay",
                "Select who should see the legislative area contact details");
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var documentLegislativeArea = latestDocument!.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == laId);
        if (vm.ReviewDate != null && vm.ReviewDate != documentLegislativeArea.ReviewDate && string.IsNullOrWhiteSpace(vm.UserNotes))
        {
            vm.UserNotesLabel = UserNotesLabelMandatory;
            ModelState.AddModelError("UserNotes",
                "Enter user notes");
        }
        
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

       
        documentLegislativeArea.IsProvisional = vm.IsProvisionalLegislativeArea;
        documentLegislativeArea.AppointmentDate = vm.AppointmentDate;
        documentLegislativeArea.ReviewDate = vm.ReviewDate;
        documentLegislativeArea.UserNotes = vm.UserNotes;
        documentLegislativeArea.Reason = vm.Reason;
        documentLegislativeArea.PointOfContactName = vm.PointOfContactName;
        documentLegislativeArea.PointOfContactEmail = vm.PointOfContactEmail;
        documentLegislativeArea.PointOfContactPhone = vm.PointOfContactPhone;
        documentLegislativeArea.IsPointOfContactPublicDisplay = vm.IsPointOfContactPublicDisplay;

        // Note: After editing a published/declined LA in a new draft cab, the LA status is changed to draft.
        if (latestDocument.StatusValue == Status.Draft && latestDocument.SubStatus == SubStatus.None &&
            (documentLegislativeArea.Status == LAStatus.Published || documentLegislativeArea.Status == LAStatus.Declined || documentLegislativeArea.Status == LAStatus.DeclinedByOpssAdmin))
        {
            documentLegislativeArea.Status = LAStatus.Draft;
        }

        var userAccount =
            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

        const string reviewDateReason =
            "The review date for the {0} legislative area was changed for the following reason: {1}";

        if (!string.IsNullOrWhiteSpace(vm.UserNotes))
        {
            latestDocument.AuditLog.Add(new Audit(userAccount: userAccount,
                action: AuditCABActions.LegislativeAreaAdditionalInformationUserNotes,
                comment: vm.UserNotes,
                publicComment: string.IsNullOrWhiteSpace(vm.Reason)
                    ? null
                    : string.Format(reviewDateReason, documentLegislativeArea.LegislativeAreaName, vm.Reason)
            ));
        }

        if (!string.IsNullOrWhiteSpace(vm.Reason))
        {
            latestDocument.AuditLog.Add(new Audit(userAccount: userAccount,
                action: AuditCABActions.LegislativeAreaAdditionalInformationReason,
                publicComment: string.Format(reviewDateReason, documentLegislativeArea.LegislativeAreaName, vm.Reason)
            ));
        }

        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
        
        TempData.Remove(StoragePageTitle);
        return submitType switch
        {
            Constants.SubmitType.Continue => RedirectToRoute(
                LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary }),
            _ => RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true })
        };
    }
}