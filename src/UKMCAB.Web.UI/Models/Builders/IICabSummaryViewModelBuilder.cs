using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabSummaryViewModelBuilder
    {
        ICabSummaryViewModelBuilder WithIds(string id, string cabid);
        ICabSummaryViewModelBuilder WithCabDetails(CABDetailsViewModel cabDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabContactViewModel(CABContactViewModel cabContactViewModel);
        ICabSummaryViewModelBuilder WithCabBodyDetailsViewModel(CABBodyDetailsViewModel cabBodyDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabLegislativeAreasViewModel(CABLegislativeAreasViewModel cabLegislativeAreasViewModel);
        ICabSummaryViewModelBuilder WithProductScheduleDetailsViewModel(CABProductScheduleDetailsViewModel cabProductScheduleDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabSupportingDocumentDetailsViewModel(CABSupportingDocumentDetailsViewModel cabSupportingDocumentDetailsViewModel);
        ICabSummaryViewModelBuilder WithReturnUrl(string? returnUrl);
        ICabSummaryViewModelBuilder WithCabNameAlreadyExists(bool cabNameAlreadyExists, Status documentStatus);
        ICabSummaryViewModelBuilder WithStatus(Status documentStatusValue, SubStatus documentSubStatus);
        ICabSummaryViewModelBuilder WithStatusCssStyle(Status documentStatusValue);
        ICabSummaryViewModelBuilder WithHasActiveLAs(bool documentHasActiveLAs);
        ICabSummaryViewModelBuilder WithIsEditLocked(bool isCabLockedForUser);
        ICabSummaryViewModelBuilder WithSubSectionEditAllowed(bool? subSectionEditAllowed);
        ICabSummaryViewModelBuilder WithLastModifiedDate(DateTime documentLastUpdatedDate);
        ICabSummaryViewModelBuilder WithPublishedDate(List<Audit> auditLog);
        ICabSummaryViewModelBuilder WithGovernmentUserNotes(List<UserNote> userNotes);
        ICabSummaryViewModelBuilder WithLastAuditLogHistoryDate(List<Audit> auditLog);
        ICabSummaryViewModelBuilder WithIsPendingOgdApproval(bool documentIsPendingOgdApproval);
        ICabSummaryViewModelBuilder WithLegislativeAreasPendingApprovalCount(Document document);
        ICabSummaryViewModelBuilder WithLegislativeAreasApprovedByAdminCount(int legislativeAreasApprovedByAdminCount);
        ICabSummaryViewModelBuilder WithLegislativeAreaHasBeenActioned(bool legislativeAreaHasBeenActioned);
        ICabSummaryViewModelBuilder WithHasActionableLegislativeAreaForOpssAdmin(bool hasActionableLegislativeAreaForOpssAdmin);
        ICabSummaryViewModelBuilder WithRequestedFromCabProfilePage(bool? fromCabProfilePage);
        ICabSummaryViewModelBuilder WithDraftUpdated(List<Audit> documentAuditLog, DateTime documentLastUpdatedDate);
        ICabSummaryViewModelBuilder WithRoleInfo(string documentCreatedByUserGroup);
        ICabSummaryViewModelBuilder WithSuccessBannerMessage(string? message);
        CABSummaryViewModel Build();
    }
}
