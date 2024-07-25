using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabSummaryViewModelBuilder
    {
        ICabSummaryViewModelBuilder WithDocumentDetails(Document document);
        ICabSummaryViewModelBuilder WithLegislativeAreasPendingApprovalCount(Document document);
        ICabSummaryViewModelBuilder WithReturnUrl(string? returnUrl);
        ICabSummaryViewModelBuilder WithCabDetails(CABDetailsViewModel cabDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabContactViewModel(CABContactViewModel cabContactViewModel);
        ICabSummaryViewModelBuilder WithCabBodyDetailsViewModel(CABBodyDetailsViewModel cabBodyDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabLegislativeAreasViewModel(CABLegislativeAreasViewModel cabLegislativeAreasViewModel);
        ICabSummaryViewModelBuilder WithProductScheduleDetailsViewModel(CABProductScheduleDetailsViewModel cabProductScheduleDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabSupportingDocumentDetailsViewModel(CABSupportingDocumentDetailsViewModel cabSupportingDocumentDetailsViewModel);
        ICabSummaryViewModelBuilder WithCabHistoryViewModel(CABHistoryViewModel cabHistoryViewModel);
        ICabSummaryViewModelBuilder WithCabGovernmentUserNotesViewModel(CABGovernmentUserNotesViewModel cabGovernmentUserNotesViewModel);
        ICabSummaryViewModelBuilder WithIsEditLocked(bool isEditLocked);
        ICabSummaryViewModelBuilder WithRoleInfo(Document document);
        ICabSummaryViewModelBuilder WithSuccessBannerMessage(string? message);
        ICabSummaryViewModelBuilder WithRevealEditActions(bool? revealEditActions);
        ICabSummaryViewModelBuilder WithRequestedFromCabProfilePage(bool? fromCabProfilePage);
        CABSummaryViewModel Build();
    }
}
