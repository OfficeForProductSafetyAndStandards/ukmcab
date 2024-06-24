using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Claims;
using System.Threading.Channels;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using static UKMCAB.Web.UI.Constants;

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
        public const string LegislativeAreaAdditionalInformationReviewDateNotes = "legislative.area.additional.information.review.date.notes";
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

        if (submitType == SubmitType.Add18)
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

        if ((!string.IsNullOrWhiteSpace(vm.PointOfContactName) ||
            !string.IsNullOrWhiteSpace(vm.PointOfContactEmail) ||
            !string.IsNullOrWhiteSpace(vm.PointOfContactPhone)) &&
                !vm.IsPointOfContactPublicDisplay.HasValue)
        {
            ModelState.AddModelError("IsPointOfContactPublicDisplay",
                "Select who should see the legislative area contact details");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var documentLegislativeArea = latestDocument!.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == laId);

        documentLegislativeArea.IsProvisional = vm.IsProvisionalLegislativeArea;
        documentLegislativeArea.AppointmentDate = vm.AppointmentDate;
        documentLegislativeArea.PointOfContactName = vm.PointOfContactName;
        documentLegislativeArea.PointOfContactEmail = vm.PointOfContactEmail;
        documentLegislativeArea.PointOfContactPhone = vm.PointOfContactPhone;
        documentLegislativeArea.IsPointOfContactPublicDisplay = vm.IsPointOfContactPublicDisplay;
        documentLegislativeArea.MarkAsDraft(latestDocument);

        var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

        if (documentLegislativeArea.ReviewDate == null)
        {
            documentLegislativeArea.ReviewDate = vm.ReviewDate;
            latestDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.LegislativeAreaReviewDateAdded));
        }

        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
        
        TempData.Remove(StoragePageTitle);

        if (vm.ReviewDate != null && vm.ReviewDate != documentLegislativeArea.ReviewDate)
        {
            vm.SubmitType = submitType;
            return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewDateNotes.cshtml", new ReviewDateNotesViewModel
            {
                CabId = vm.CabId,
                LegislativeAreaId = vm.LegislativeAreaId,
                LegislativeAreaName = documentLegislativeArea.LegislativeAreaName,
                ReviewDate = vm.ReviewDate,
                FromSummary = vm.IsFromSummary
            });
        }

        return submitType switch
        {
            SubmitType.Continue => RedirectToRoute(
                LegislativeAreaReviewController.Routes.ReviewLegislativeAreas, new { id, fromSummary = vm.IsFromSummary }),
            _ => RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true })
        };
    }

    [HttpPost("additional-information/{laId}/review-date-notes", Name = Routes.LegislativeAreaAdditionalInformationReviewDateNotes)]
    public async Task<IActionResult> ReviewDateNotes(ReviewDateNotesViewModel vm,
        Guid id, Guid laId, string submitType, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(vm.UserNotes))
        {
            ModelState.AddModelError("UserNotes", "Enter user notes");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewDateNotes.cshtml", vm);
        }

        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        var documentLegislativeArea = latestDocument!.DocumentLegislativeAreas.First(a => a.LegislativeAreaId == laId);

        documentLegislativeArea.ReviewDate = vm.ReviewDate;
        documentLegislativeArea.UserNotes = vm.UserNotes;
        documentLegislativeArea.Reason = vm.Reason;
        documentLegislativeArea.MarkAsDraft(latestDocument);

        var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
        
        latestDocument.AuditLog.Add(new Audit(
            userAccount, 
            AuditCABActions.LegislativeAreaReviewDateUpdated, 
            $"Changed review date on legislative area {vm.LegislativeAreaName}\r\n{vm.UserNotes}", 
            vm.Reason
        ));

        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

        return RedirectToRoute(CABController.Routes.CabSummary, new { id, subSectionEditAllowed = true });
    }
}