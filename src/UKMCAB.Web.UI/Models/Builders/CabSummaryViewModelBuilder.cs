using System.Net;
using System.Security.Claims;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Extensions;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Models.Builders
{
    public class CabSummaryViewModelBuilder : ICabSummaryViewModelBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICabLegislativeAreasItemViewModelBuilder _cabLegislativeAreasItemViewModelBuilder;
        private readonly ClaimsPrincipal _user;
        private CABSummaryViewModel _model { get; set; }

        public CabSummaryViewModelBuilder(ICabLegislativeAreasItemViewModelBuilder cabLegislativeAreasItemViewModelBuilder, IHttpContextAccessor httpContextAccessor)
        {
            _cabLegislativeAreasItemViewModelBuilder = cabLegislativeAreasItemViewModelBuilder;
            _httpContextAccessor = httpContextAccessor;
            _user = _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("No active HttpContext");
            _model = new CABSummaryViewModel();
        }

        public CABSummaryViewModel Build()
        {
            var model = _model;
            _model = new CABSummaryViewModel();
            return model;
        }

        public ICabSummaryViewModelBuilder WithDocumentDetails(Document document)
        {
            _model.Id = document.id;
            _model.CABId = document.CABId;
            _model.Status = document.StatusValue;
            _model.SubStatus = document.SubStatus;
            _model.SubStatusName = document.SubStatus.GetEnumDescription();
            _model.StatusCssStyle = CssClassUtils.CabStatusStyle(document.StatusValue);
            _model.HasActiveLAs = document.HasActiveLAs();
            _model.LastModifiedDate = document.LastUpdatedDate;
            _model.PublishedDate = document.PublishedDate();
            _model.IsPendingOgdApproval = document.IsPendingOgdApproval();
            _model.LegislativeAreasApprovedByAdminCount = document.LegislativeAreasApprovedByAdminCount();
            _model.LegislativeAreaHasBeenActioned = document.LegislativeAreaHasBeenActioned();
            _model.HasActionableLegislativeAreaForOpssAdmin = document.HasActionableLegislativeAreaForOpssAdmin();
            _model.DraftUpdated = document.DraftUpdated();
            return this;
        }

        public ICabSummaryViewModelBuilder WithLegislativeAreasPendingApprovalCount(Document document)
        {
            _model.LegislativeAreasPendingApprovalForCurrentUserCount = !_user.IsInRole(Roles.OPSS.Id)
                ? document.LegislativeAreasPendingApprovalByOgd(_user.GetRoleId()).Count
                : document.LegislativeAreasPendingApprovalByOpss().Count;
            return this;
        }

        public ICabSummaryViewModelBuilder WithReturnUrl(string? returnUrl)
        {
            var decodedUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? WebUtility.UrlDecode(string.Empty)
                : WebUtility.UrlDecode(returnUrl);

            _model.ReturnUrl = decodedUrl == "/" ? "/?" : decodedUrl;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabDetails(CABDetailsViewModel cabDetailsViewModel)
        {
            _model.CabDetailsViewModel = cabDetailsViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabContactViewModel(CABContactViewModel cabContactViewModel)
        {
            _model.CabContactViewModel = cabContactViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabBodyDetailsViewModel(CABBodyDetailsViewModel cabBodyDetailsViewModel)
        {
            _model.CabBodyDetailsViewModel = cabBodyDetailsViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabLegislativeAreasViewModel(CABLegislativeAreasViewModel cabLegislativeAreasViewModel)
        {
            _model.CabLegislativeAreasViewModel = cabLegislativeAreasViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithProductScheduleDetailsViewModel(CABProductScheduleDetailsViewModel cabProductScheduleDetailsViewModel)
        {
            _model.CABProductScheduleDetailsViewModel = cabProductScheduleDetailsViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabSupportingDocumentDetailsViewModel(CABSupportingDocumentDetailsViewModel cabSupportingDocumentDetailsViewModel)
        {
            _model.CABSupportingDocumentDetailsViewModel = cabSupportingDocumentDetailsViewModel;
            return this;
        }
        public ICabSummaryViewModelBuilder WithCabHistoryViewModel(CABHistoryViewModel cabHistoryViewModel)
        {
            _model.CABHistoryViewModel = cabHistoryViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithCabGovernmentUserNotesViewModel(CABGovernmentUserNotesViewModel cabGovernmentUserNotesViewModel)
        {
            _model.CABGovernmentUserNotesViewModel = cabGovernmentUserNotesViewModel;
            return this;
        }

        public ICabSummaryViewModelBuilder WithIsEditLocked(bool isEditLocked)
        {
            _model.IsEditLocked = isEditLocked;
            return this;
        }

        public ICabSummaryViewModelBuilder WithRevealEditActions(bool? revealEditActions)
        {
            _model.RevealEditActions = revealEditActions ?? false;
            return this;
        }
        public ICabSummaryViewModelBuilder WithRoleInfo(Document document)
        {
            _model.UserInCreatorUserGroup = _user.GetRoleId() == document.CreatedByUserGroup;
            _model.IsOpssAdmin = _user.IsInRole(Roles.OPSS.Id);
            _model.IsUkas = _user.IsInRole(Roles.UKAS.Id);
            _model.IsOPSSOrInCreatorUserGroup = _model.IsOpssAdmin || _model.UserInCreatorUserGroup;
            _model.HasOgdRole = _user.HasOgdRole();
            return this;
        }

        public ICabSummaryViewModelBuilder WithSuccessBannerMessage(string? message)
        {
            _model.SuccessBannerMessage = message;
            return this;
        }

        public ICabSummaryViewModelBuilder WithRequestedFromCabProfilePage(bool? fromCabProfilePage)
        {
            _model.RequestedFromCabProfilePage = fromCabProfilePage ?? false;
            return this;
        }
    }
}
