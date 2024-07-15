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

        public ICabSummaryViewModelBuilder WithIds(string id, string cabid)
        {
            _model.Id = id;
            _model.CABId = cabid;
            return this;
        }

        public ICabSummaryViewModelBuilder WithReturnUrl(string? returnUrl)
        {
            _model.ReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? WebUtility.UrlDecode(string.Empty)
                : WebUtility.UrlDecode(returnUrl);
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

        public ICabSummaryViewModelBuilder WithCabNameAlreadyExists(bool cabNameAlreadyExists, Status documentStatusValue)
        {
            _model.CABNameAlreadyExists = cabNameAlreadyExists && documentStatusValue != Status.Published;
            return this;
        }

        public ICabSummaryViewModelBuilder WithStatus(Status documentStatusValue, SubStatus documentSubStatus)
        {
            _model.Status = documentStatusValue;
            _model.SubStatus = documentSubStatus;
            _model.SubStatusName = documentSubStatus.GetEnumDescription();
            return this;
        }

        public ICabSummaryViewModelBuilder WithStatusCssStyle(Status documentStatusValue)
        {
            _model.StatusCssStyle = CssClassUtils.CabStatusStyle(documentStatusValue);
            return this;
        }

        public ICabSummaryViewModelBuilder WithHasActiveLAs(bool documentHasActiveLAs)
        {
            _model.HasActiveLAs = documentHasActiveLAs;
            return this;
        }

        public ICabSummaryViewModelBuilder WithIsEditLocked(bool isEditLocked)
        {
            _model.IsEditLocked = isEditLocked;
            return this;
        }

        public ICabSummaryViewModelBuilder WithSubSectionEditAllowed(bool? subSectionEditAllowed)
        {
            _model.SubSectionEditAllowed = subSectionEditAllowed ?? false;
            return this;
        }

        public ICabSummaryViewModelBuilder WithLastModifiedDate(DateTime documentLastUpdatedDate)
        {
            _model.LastModifiedDate = documentLastUpdatedDate;
            return this;
        }

        public ICabSummaryViewModelBuilder WithPublishedDate(List<Audit> auditLog)
        {
            _model.PublishedDate = auditLog.OrderBy(a => a.DateTime).LastOrDefault(al => al.Action == AuditCABActions.Published)?.DateTime;
            return this;
        }

        public ICabSummaryViewModelBuilder WithGovernmentUserNotes(List<UserNote> documentUserNotes)
        {
            _model.GovernmentUserNoteCount = documentUserNotes.Count;
            _model.LastGovernmentUserNoteDate = Enumerable.MaxBy(documentUserNotes, u => u.DateTime)?.DateTime;
            return this;
        }

        public ICabSummaryViewModelBuilder WithLastAuditLogHistoryDate(List<Audit> documentAuditLog)
        {
            _model.LastAuditLogHistoryDate = Enumerable.MaxBy(documentAuditLog, u => u.DateTime)?.DateTime;
            return this;
        }

        public ICabSummaryViewModelBuilder WithIsPendingOgdApproval(bool documentIsPendingOgdApproval)
        {
            _model.IsPendingOgdApproval = documentIsPendingOgdApproval;
            return this;
        }

        public ICabSummaryViewModelBuilder WithLegislativeAreasPendingApprovalCount(Document document)
        {
            _model.LegislativeAreasPendingApprovalCount = !_user.IsInRole(Roles.OPSS.Id)
                ? document.GetLegislativeAreasPendingApprovalByOgd(_user.GetRoleId()).Count
                : document.GetLegislativeAreasPendingApprovalByOpss().Count;
            return this;
        }

        public ICabSummaryViewModelBuilder WithLegislativeAreasApprovedByAdminCount(int legislativeAreasApprovedByAdminCount)
        {
            _model.LegislativeAreasApprovedByAdminCount = legislativeAreasApprovedByAdminCount;
            return this;
        }

        public ICabSummaryViewModelBuilder WithLegislativeAreaHasBeenActioned(bool legislativeAreaHasBeenActioned)
        {
            _model.LegislativeAreaHasBeenActioned = legislativeAreaHasBeenActioned;
            return this;
        }

        public ICabSummaryViewModelBuilder WithHasActionableLegislativeAreaForOpssAdmin(bool hasActionableLegislativeAreaForOpssAdmin)
        {
            _model.HasActionableLegislativeAreaForOpssAdmin = hasActionableLegislativeAreaForOpssAdmin;
            return this;
        }

        public ICabSummaryViewModelBuilder WithRequestedFromCabProfilePage(bool? fromCabProfilePage)
        {
            _model.RequestedFromCabProfilePage = fromCabProfilePage ?? false;
            return this;
        }

        public ICabSummaryViewModelBuilder WithDraftUpdated(List<Audit> documentAuditLog, DateTime documentLastUpdatedDate)
        {
            _model.DraftUpdated = Enumerable.MaxBy(
                documentAuditLog.Where(l => l.Action == AuditCABActions.Created),
                u => u.DateTime)?.DateTime != documentLastUpdatedDate;
            return this;
        }

        public ICabSummaryViewModelBuilder WithRoleInfo(string documentCreatedByUserGroup)
        {
            _model.IsOpssAdmin = _user.IsInRole(Roles.OPSS.Id);
            _model.IsUkas = _user.IsInRole(Roles.UKAS.Id);
            _model.UserInCreatorUserGroup = _user.GetRoleId() == documentCreatedByUserGroup;
            _model.IsOPSSOrInCreatorUserGroup = _model.IsOpssAdmin || _model.UserInCreatorUserGroup;
            _model.HasOgdRole = _user.HasOgdRole();
            return this;
        }

        public ICabSummaryViewModelBuilder WithSuccessBannerMessage(string? message)
        {
            _model.SuccessBannerMessage = message;
            return this;
        }
    }
}
