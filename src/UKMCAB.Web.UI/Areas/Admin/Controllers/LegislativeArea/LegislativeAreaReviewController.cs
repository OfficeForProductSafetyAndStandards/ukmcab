using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea.Review;
using UKMCAB.Core.Extensions;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area/review-legislative-areas"), Authorize(Policy = Policies.LegislativeAreaManage)]
public class LegislativeAreaReviewController : UI.Controllers.ControllerBase
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;

    public LegislativeAreaReviewController(
        ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService) : base(userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
    }

    public static class Routes
    {
        public const string ReviewLegislativeAreas = "legislative.area.selected";
    }

    [HttpGet(Name = Routes.ReviewLegislativeAreas)]
    public async Task<IActionResult> ReviewLegislativeAreas(Guid id, string? returnUrl, LegislativeAreaActionMessageEnum? actionType, bool fromSummary, string? bannerContent)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());
        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);
        var singleDraftDoc = await _cabAdminService.IsSingleDraftDocAsync(id);
        vm.ShowArchiveLegislativeAreaAction = !singleDraftDoc;

        if (actionType.HasValue)
        {
            vm.SuccessBannerMessage = AlertMessagesUtils.LegislativeAreaActionMessages[actionType.Value];
        }
        if (!string.IsNullOrWhiteSpace(bannerContent))
        {
            vm.BannerContent = bannerContent;
        }
        vm.FromSummary = fromSummary;
        vm.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;

        if (fromSummary) { vm.SubTitle = "Edit a CAB"; }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
    }

    [HttpPost(Name = Routes.ReviewLegislativeAreas)]
    public async Task<IActionResult> ReviewLegislativeAreas(Guid id, string submitType, ReviewLegislativeAreasViewModel reviewLaVM)
    {
        var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id.ToString());

        // Implies no document or archived
        if (latestDocument == null)
        {
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        if(submitType == "AddLegislativeArea")
        {
            var cabLegislativeAreaIds = latestDocument.DocumentLegislativeAreas.Select(n => n.LegislativeAreaId).ToList();
            var remainingLegislativeAreasToSelect = await _legislativeAreaService.GetLegislativeAreasAsync(cabLegislativeAreaIds);

            // if all legislative area added and none left to add
            if (remainingLegislativeAreasToSelect.Count() == 0)
            {
                ModelState.AddModelError("AddLegislativeArea", "All legislative areas have already been added to this CAB profile.");
                var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);                
                return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
            }
            else
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddLegislativeArea, new { id, fromSummary = reviewLaVM.FromSummary });
            }
        }

        var laIdOrLaName = string.Empty;

        if (submitType.StartsWith("Add-"))
        {
            laIdOrLaName = submitType[4..];
            if (Guid.TryParse(laIdOrLaName, out Guid laId))
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddPurposeOfAppointment, new { id, scopeId = Guid.Empty, legislativeAreaId = laId, fromSummary = reviewLaVM.FromSummary });
            }
        }
        else if (submitType.StartsWith("Edit-"))
        {
            laIdOrLaName = submitType[5..];
        }
        else if (submitType.StartsWith("Remove-"))
        {
            laIdOrLaName = submitType[7..];
        }

        var cabLaOfSelectedScopeofAppointment = reviewLaVM.ActiveLAItems.FirstOrDefault(la => la.SelectedScopeOfAppointmentId != null && la.Name == laIdOrLaName);
        if (cabLaOfSelectedScopeofAppointment == null)
        {
            ModelState.AddModelError(laIdOrLaName, "Select a scope of appointment");
            var isOgdUser = Roles.OgdRolesList.Contains(UserRoleId);
            if (isOgdUser)
            {
                await _cabAdminService.FilterCabContentsByLaIfPendingOgdApproval(latestDocument, UserRoleId);
            }
            var vm = await PopulateCABLegislativeAreasViewModelAsync(latestDocument);
            vm.ErrorLink = laIdOrLaName;
            return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", vm);
        }

        var laOfSelectedSoa = cabLaOfSelectedScopeofAppointment.ScopeOfAppointments.First(soa => soa.ScopeId == cabLaOfSelectedScopeofAppointment.SelectedScopeOfAppointmentId);
        var legislativeAreaId = laOfSelectedSoa.LegislativeArea.Id;
        var selectedScopeOfAppointmentId = laOfSelectedSoa.ScopeId;
        Guard.IsTrue(selectedScopeOfAppointmentId != Guid.Empty, "Scope Id Guid cannot be empty");

        if (ModelState.IsValid)
        {
            if (submitType.StartsWith(Constants.SubmitType.Edit))
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddPurposeOfAppointment, new { id, scopeId = Guid.Empty, compareScopeId = selectedScopeOfAppointmentId, legislativeAreaId, fromSummary = reviewLaVM.FromSummary });
            }
            if (submitType.StartsWith(Constants.SubmitType.Remove))
            {
                var soaToRemove = latestDocument.ScopeOfAppointments.First(s => s.Id == selectedScopeOfAppointmentId);
                if (soaToRemove != null)
                {
                    var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                    latestDocument.ScopeOfAppointments.Remove(soaToRemove);

                    var documentLegislativeArea = latestDocument.DocumentLegislativeAreas.FirstOrDefault(la => la.LegislativeAreaId == legislativeAreaId);
                    documentLegislativeArea?.MarkAsDraft(latestDocument.StatusValue, latestDocument.SubStatus);

                    latestDocument.AuditLog.Add(new Audit(userAccount, AuditCABActions.ScopeOfAppointmentRemovedFrom(documentLegislativeArea?.LegislativeAreaName)));

                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

                    return RedirectToRoute(Routes.ReviewLegislativeAreas, new { id, actionType = LegislativeAreaActionMessageEnum.ScopeOfAppointmentRemoved, fromSummary = reviewLaVM.FromSummary });
                }
            }
        }

        return View("~/Areas/Admin/views/CAB/LegislativeArea/ReviewLegislativeAreas.cshtml", new ReviewLegislativeAreasViewModel());
    }

    private async Task<ReviewLegislativeAreasViewModel> PopulateCABLegislativeAreasViewModelAsync(Document cab)
    {
        var viewModel = new ReviewLegislativeAreasViewModel
        {
            CABId = Guid.Parse(cab.CABId),
            ShowAddRemoveLegislativeAreaActions = !cab.IsPendingOgdApproval(),
        };

        foreach (var documentLegislativeArea in cab.DocumentLegislativeAreas)
        {
            var legislativeArea =
                await _legislativeAreaService.GetLegislativeAreaByIdAsync(documentLegislativeArea
                    .LegislativeAreaId);

            var legislativeAreaViewModel = new CABLegislativeAreasItemViewModel
            {
                LegislativeAreaId = legislativeArea.Id,
                Name = legislativeArea.Name,
                Status = documentLegislativeArea.Status,
                RoleName = Roles.NameFor(documentLegislativeArea.RoleId),
                IsProvisional = documentLegislativeArea.IsProvisional,
                AppointmentDate = documentLegislativeArea.AppointmentDate,
                ReviewDate = documentLegislativeArea.ReviewDate,
                Reason = documentLegislativeArea.Reason,
                CanChooseScopeOfAppointment = legislativeArea.HasDataModel,
                IsArchived = documentLegislativeArea.Archived,
                ShowEditActions = documentLegislativeArea.Status == LAStatus.Draft || 
                                  documentLegislativeArea.Status == LAStatus.PendingApproval && UserRoleId == documentLegislativeArea.RoleId ||
                                  documentLegislativeArea.Status == LAStatus.Approved && UserRoleId == Roles.OPSS.Id ||
                                  cab.StatusValue == Status.Draft && cab.SubStatus == SubStatus.None,
            };

            var scopeOfAppointments = cab.ScopeOfAppointments.Where(x => x.LegislativeAreaId == legislativeArea.Id);
            foreach (var scopeOfAppointment in scopeOfAppointments)
            {
                var purposeOfAppointment = scopeOfAppointment.PurposeOfAppointmentId.HasValue
                    ? (await _legislativeAreaService.GetPurposeOfAppointmentByIdAsync(scopeOfAppointment
                        .PurposeOfAppointmentId.Value))?.Name
                    : null;

                var category = scopeOfAppointment.CategoryId.HasValue
                    ? (await _legislativeAreaService.GetCategoryByIdAsync(scopeOfAppointment.CategoryId.Value))
                    : null;

                var subCategory = scopeOfAppointment.SubCategoryId.HasValue
                    ? (await _legislativeAreaService.GetSubCategoryByIdAsync(scopeOfAppointment.SubCategoryId
                        .Value))?.Name
                    : null;

                var ppeProductType = scopeOfAppointment.PpeProductTypeId.HasValue
                    ? (await _legislativeAreaService.GetPpeProductTypeByIdAsync(scopeOfAppointment.PpeProductTypeId
                        .Value))?.Name
                    : null;

                var protectionAgainstRisk = scopeOfAppointment.ProtectionAgainstRiskId.HasValue
                    ? (await _legislativeAreaService.GetProtectionAgainstRiskByIdAsync(scopeOfAppointment.ProtectionAgainstRiskId
                        .Value))?.Name
                    : null;

                foreach (var productProcedure in scopeOfAppointment.ProductIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        Category = category?.Name,
                        SubCategory = subCategory,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (productProcedure.ProductId.HasValue)
                    {
                        var product =
                            await _legislativeAreaService.GetProductByIdAsync(productProcedure.ProductId.Value);
                        soaViewModel.Product = product!.Name;
                    }

                    foreach (var procedureId in productProcedure.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                foreach (var categoryProcedure in scopeOfAppointment.CategoryIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        Category = category?.Name,
                        SubCategory = subCategory,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (categoryProcedure.CategoryId.HasValue)
                    {
                        category = await _legislativeAreaService.GetCategoryByIdAsync(categoryProcedure.CategoryId.Value);
                        soaViewModel.Category = category!.Name;
                    }

                    foreach (var procedureId in categoryProcedure.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                foreach (var ppeProductTypeIdAndProcedureIds in scopeOfAppointment.PpeProductTypeIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (ppeProductTypeIdAndProcedureIds.PpeProductTypeId.HasValue)
                    {
                        var selectedPpeProductType =
                            await _legislativeAreaService.GetPpeProductTypeByIdAsync(ppeProductTypeIdAndProcedureIds.PpeProductTypeId.Value);
                        soaViewModel.PpeProductType = selectedPpeProductType!.Name;
                    }

                    foreach (var procedureId in ppeProductTypeIdAndProcedureIds.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                foreach (var protectionAgainstRiskIdAndProcedureIds in scopeOfAppointment.ProtectionAgainstRiskIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (protectionAgainstRiskIdAndProcedureIds.ProtectionAgainstRiskId.HasValue)
                    {
                        var selectedProtectionAgainstRiskIdAndProcedureIds =
                            await _legislativeAreaService.GetProtectionAgainstRiskByIdAsync(protectionAgainstRiskIdAndProcedureIds.ProtectionAgainstRiskId.Value);
                        soaViewModel.ProtectionAgainstRisk = selectedProtectionAgainstRiskIdAndProcedureIds!.Name;
                    }

                    foreach (var procedureId in protectionAgainstRiskIdAndProcedureIds.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                foreach (var areaOfCompetencyProcedure in scopeOfAppointment.AreaOfCompetencyIdAndProcedureIds)
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        PurposeOfAppointment = purposeOfAppointment,
                        PpeProductType = ppeProductType,
                        ProtectionAgainstRisk = protectionAgainstRisk,
                        ScopeId = scopeOfAppointment.Id,
                    };

                    if (areaOfCompetencyProcedure.AreaOfCompetencyId.HasValue)
                    {
                        var areaOfCompetency =
                            await _legislativeAreaService.GetAreaOfCompetencyByIdAsync(areaOfCompetencyProcedure.AreaOfCompetencyId.Value);
                        soaViewModel.AreaOfCompetency = areaOfCompetency!.Name;
                    }

                    foreach (var procedureId in areaOfCompetencyProcedure.ProcedureIds)
                    {
                        var procedure = await _legislativeAreaService.GetProcedureByIdAsync(procedureId);
                        soaViewModel.Procedures?.Add(procedure!.Name);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }

                if (scopeOfAppointment.DesignatedStandardIds.Any())
                {
                    var soaViewModel = new LegislativeAreaListItemViewModel
                    {
                        LegislativeArea = new ListItem { Id = legislativeArea.Id, Title = legislativeArea.Name },
                        DesignatedStandards = new(),
                        ScopeId = scopeOfAppointment.Id,
                    };

                    foreach (var designatedStandardId in scopeOfAppointment.DesignatedStandardIds)
                    {
                        var designatedStandard = await _legislativeAreaService.GetDesignatedStandardByIdAsync(designatedStandardId);

                        var designatedStandardReadOnlyVM = new DesignatedStandardReadOnlyViewModel(
                            designatedStandard.Id, designatedStandard.Name, designatedStandard.ReferenceNumber, designatedStandard.NoticeOfPublicationReference);

                        soaViewModel.DesignatedStandards.Add(designatedStandardReadOnlyVM);
                    }

                    legislativeAreaViewModel.ScopeOfAppointments.Add(soaViewModel);
                }
            }

            if (legislativeAreaViewModel.Name.Equals(Constants.PpeLaName))
            {
                var sortedLAViewModels = new List<LegislativeAreaListItemViewModel>();
                if (legislativeAreaViewModel.ShowPpeProductTypeColumn)
                {
                    var laItemListVMs = legislativeAreaViewModel.ScopeOfAppointments.Where(l => l.PpeProductType != null);
                    sortedLAViewModels.AddRange(laItemListVMs);
                }

                if (legislativeAreaViewModel.ShowProtectionAgainstRiskColumn)
                {
                    var laItemListVMs = legislativeAreaViewModel.ScopeOfAppointments.Where(l => l.ProtectionAgainstRisk != null);
                    sortedLAViewModels.AddRange(laItemListVMs);
                }

                if (legislativeAreaViewModel.ShowAreaOfCompetencyColumn)
                {
                    var laItemListVMs = legislativeAreaViewModel.ScopeOfAppointments.Where(l => l.AreaOfCompetency != null);
                    sortedLAViewModels.AddRange(laItemListVMs);
                }

                legislativeAreaViewModel.ScopeOfAppointments = sortedLAViewModels;
            }

            var distinctSoa = legislativeAreaViewModel.ScopeOfAppointments.GroupBy(s => s.ScopeId).ToList();
            foreach (var item in distinctSoa)
            {
                var scopeOfApps = legislativeAreaViewModel.ScopeOfAppointments;
                scopeOfApps.First(soa => soa.ScopeId == item.Key).NoOfProductsInScopeOfAppointment = scopeOfApps.Count(soa => soa.ScopeId == item.Key);
            }
            if (legislativeAreaViewModel.IsArchived != true)
            {
                viewModel.ActiveLAItems.Add(legislativeAreaViewModel);
            }
            else
            {
                viewModel.ArchivedLAItems.Add(legislativeAreaViewModel);
            }
        };

        return viewModel;
    }
}