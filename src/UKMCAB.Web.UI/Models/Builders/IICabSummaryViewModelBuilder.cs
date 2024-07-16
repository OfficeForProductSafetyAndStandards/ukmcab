using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public interface ICabSummaryViewModelBuilder
    {
        ICabSummaryViewModelBuilder WithDocumentDetails(Document document);

        ICabSummaryViewModelBuilder WithReturnUrl(string? returnUrl);

        ICabSummaryViewModelBuilder WithCabDetails(CABDetailsViewModel cabDetailsViewModel);

        ICabSummaryViewModelBuilder WithCabContactViewModel(CABContactViewModel cabContactViewModel);

        ICabSummaryViewModelBuilder WithCabBodyDetailsViewModel(CABBodyDetailsViewModel cabBodyDetailsViewModel);

        ICabSummaryViewModelBuilder WithCabLegislativeAreasViewModel(CABLegislativeAreasViewModel cabLegislativeAreasViewModel);

        ICabSummaryViewModelBuilder WithProductScheduleDetailsViewModel(CABProductScheduleDetailsViewModel cabProductScheduleDetailsViewModel);

        ICabSummaryViewModelBuilder WithCabSupportingDocumentDetailsViewModel(CABSupportingDocumentDetailsViewModel cabSupportingDocumentDetailsViewModel);

        ICabSummaryViewModelBuilder WithCabNameAlreadyExists(bool cabNameAlreadyExists);

        ICabSummaryViewModelBuilder WithIsEditLocked(bool isEditLocked);

        ICabSummaryViewModelBuilder WithSubSectionEditAllowed(bool? subSectionEditAllowed);

        ICabSummaryViewModelBuilder WithRoleInfo();

        ICabSummaryViewModelBuilder WithSuccessBannerMessage(string? message);
        ICabSummaryViewModelBuilder WithRequestedFromCabProfilePage(bool? fromCabProfilePage);
        CABSummaryViewModel Build();
    }
}
