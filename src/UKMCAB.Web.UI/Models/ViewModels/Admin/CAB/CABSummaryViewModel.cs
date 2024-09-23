using Humanizer;
using UKMCAB.Data;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABSummaryViewModel : ILayoutModel
    {
        public string? Id { get; set; }
        public Status Status { get; set; }
        public string? StatusCssStyle { get; set; }
        public SubStatus SubStatus { get; set; }
        public string? SubStatusName { get; set; }
        public string? CABId { get; set; }
        public CABDetailsViewModel? CabDetailsViewModel { get; set; }
        public CABContactViewModel? CabContactViewModel { get; set; }
        public CABBodyDetailsMRAViewModel? CabBodyDetailsMRAViewModel { get; set; }
        public CABLegislativeAreasViewModel? CabLegislativeAreasViewModel { get; set; }
        public CABProductScheduleDetailsViewModel? CABProductScheduleDetailsViewModel { get; set; }
        public CABSupportingDocumentDetailsViewModel? CABSupportingDocumentDetailsViewModel { get; set; }
        public CABHistoryViewModel? CABHistoryViewModel { get; set; }
        public CABGovernmentUserNotesViewModel? CABGovernmentUserNotesViewModel { get; set; }
        public CABPublishTypeViewModel? CABPublishTypeViewModel { get; set; }
        public string? SelectedPublishType { get; set; }
        public string TitleHint { get; set; } = "CAB profile";
        public string? ReturnUrl { get; set; }
        public bool? FromCabProfilePage { get; set; }
        public bool ShowSubstatusName => !string.IsNullOrWhiteSpace(SubStatusName) && SubStatus != SubStatus.None;
        public bool IsPendingOgdApproval { get; set; }
        public bool IsOPSSOrInCreatorUserGroup { get; set; }
        public bool IsEditLocked { get; set; }
        public bool RevealEditActions { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? SuccessBannerMessage { get; set; }
        public int LegislativeAreasPendingApprovalForCurrentUserCount { get; set; }
        public bool IsOpssAdmin { get; set; }
        public bool IsUkas { get; set; }
        public bool UserInCreatorUserGroup { get; set; }
        public bool HasOgdRole { get; set; }
        public int LegislativeAreasApprovedByAdminCount { get; set; }
        public bool LegislativeAreaHasBeenActioned { get; set; }
        public bool HasActionableLegislativeAreaForOpssAdmin { get; set; }
        public bool IsActionableByOpssAdmin { get; set; }
        public bool RequestedFromCabProfilePage { get; set; }
        public bool HasActiveLAs { get; set; }
        public bool DraftUpdated { get; set; }

        public bool ShowSupportingDocumentsWarning =>
            RevealEditActions &&
            !IsEditLocked &&
            IsOpssAdmin && 
            (CABSupportingDocumentDetailsViewModel?.HasPublicDocuments ?? false);

        public bool ShowProductSchedulesWarning => 
            CABProductScheduleDetailsViewModel?.ActiveSchedules?.Any(p => p.CreatedBy?.Equals(Roles.UKAS.Id) ?? false) ?? false;

        public bool ShowEditButton =>
            !RevealEditActions &&
            !IsEditLocked &&
            (ShowEditButtonForOgdOrOpssNonOwner ||
            ShowEditButtonForUkasOrOpssOwner);

        public bool ShowSubSectionEditAction =>
           RevealEditActions &&
           !IsEditLocked && (           
           ShowSubSectionEditActionForOpss ||
           ShowSubSectionEditActionForOwner
           );

        public bool ShowProfileVisibilityWarning =>
            RevealEditActions &&
            !IsEditLocked &&
            ValidCAB &&
            (ShowProfileVisibilityWarningForOpssOwner ||
            ShowProfileVisibilityWarningForOpssNonOwner);

        public bool ShowMandatoryInfoWarning =>
            RevealEditActions &&
            !IsEditLocked &&
            IsOpssAdmin &&
            !ValidCAB;

        public bool ShowReviewButton =>
            RevealEditActions &&
            !IsEditLocked &&
            SubStatus != SubStatus.None &&
            LegislativeAreasPendingApprovalForCurrentUserCount > 0;

        public bool ShowApproveToPublishButton =>
            RevealEditActions &&
            !IsEditLocked &&
            !UserInCreatorUserGroup &&
            CanPublish;

        public bool ShowPublishButton =>
            ShowSubSectionEditAction &&
            SubStatus == SubStatus.None &&
            DraftUpdated &&
            IsOpssAdmin &&
            UserInCreatorUserGroup;

        public bool ShowDeclineButton => ShowApproveToPublishButton;

        public bool ShowSubmitForApprovalButton =>
            ShowSubSectionEditAction &&
            SubStatus == SubStatus.None &&  
            CanSubmitForApproval;

        public bool ShowSaveAsDraftButton =>
            ShowSubSectionEditAction &&
            SubStatus == SubStatus.None;

        public bool ShowDeleteDraftButton => 
            ShowSaveAsDraftButton ||
            (RevealEditActions &&
            IsOpssAdmin &&
            SubStatus == SubStatus.None);

        public bool ShowCancelPublishButton =>
            ShowPublishButton || 
            ShowSubmitForApprovalButton || 
            ShowDeleteDraftButton;

        public bool ValidCAB =>
            Status != Status.Published &&
            IsComplete &&
            HasActiveLAs;

        public bool CanPublish =>
            IsOpssAdmin &&
            DraftUpdated &&
            LegislativeAreasApprovedByAdminCount > 0 &&
            !CannotPublish;

        public bool CannotPublish =>
            !HasActionableLegislativeAreaForOpssAdmin &&
            IsPendingOgdApproval;

        public bool ShowOgdActions =>
            RevealEditActions &&
            !IsEditLocked &&
            HasOgdRole &&
            IsPendingOgdApproval &&
            LegislativeAreasPendingApprovalForCurrentUserCount > 0;

        public bool CanSubmitForApproval => IsUkas && DraftUpdated && IsComplete;

        public bool IsComplete =>
            CabDetailsViewModel != null &&
            CabDetailsViewModel.IsCompleted &&
            CabBodyDetailsMRAViewModel != null &&
            CabBodyDetailsMRAViewModel.IsCompleted &&
            CabContactViewModel != null &&
            CabContactViewModel.IsCompleted &&
            CabLegislativeAreasViewModel != null &&
            CabLegislativeAreasViewModel.IsCompleted &&
            CABProductScheduleDetailsViewModel != null &&
            CABProductScheduleDetailsViewModel.IsCompleted &&
            CABSupportingDocumentDetailsViewModel != null &&
            CABSupportingDocumentDetailsViewModel.IsCompleted;

        public string Title => GetTitle();

        public string GetTitle() => IsOpssAdmin
            ? SubStatus == SubStatus.PendingApprovalToPublish
                ? "Check details before approving or declining"
                : "Check details before publishing"
            : UserInCreatorUserGroup ? "Check details before submitting for approval" : "Summary";

        public bool EditByGroupPermitted =>
            SubStatus != SubStatus.PendingApprovalToPublish &&
            (Status == Status.Published || UserInCreatorUserGroup);

        public bool ShowBannerContentEmptyString =>
            !IsEditLocked &&
            (ShowBannerEmptyStringForOwner ||
            ShowBannerEmptyStringForNonOwnerOpss ||
            ShowBannerEmptyStringForNonOwnerOgd ||
            Status != Status.Draft);

        public bool ShowBannerContentCannotBeEdited =>
            ShowBannerCannotBeEditedForUkas ||
            ShowBannerCannotBeEditedForOpssAndOgd;

        private bool ShowBannerCannotBeEditedForUkas =>
            IsUkas &&
            UserInCreatorUserGroup &&
            Status == Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish;
        private bool ShowBannerCannotBeEditedForOpssAndOgd =>
            (IsOpssAdmin || HasOgdRole) &&
            Status == Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish &&
            LegislativeAreasPendingApprovalForCurrentUserCount == 0;

        private bool ShowBannerEmptyStringForOwner =>
            UserInCreatorUserGroup &&
            (IsOpssAdmin || IsUkas) &&
            Status == Status.Draft &&
            SubStatus == SubStatus.None;
        private bool ShowBannerEmptyStringForNonOwnerOgd =>
            !UserInCreatorUserGroup &&
            HasOgdRole &&
            Status == Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish &&
            LegislativeAreasPendingApprovalForCurrentUserCount > 0;
        private bool ShowBannerEmptyStringForNonOwnerOpss =>
            !UserInCreatorUserGroup &&
            IsOpssAdmin &&
            Status == Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish &&
            IsActionableByOpssAdmin;

        public string BannerContent => GetBannerContent();

        public string GetBannerContent()
        {
            if (ShowBannerContentEmptyString)
            {
                return string.Empty;
            }

            if (IsEditLocked && !IsPendingOgdApproval)
            {
                return "This CAB profile cannot be edited as it's being edited by another user.";
            }            

            if (ShowBannerContentCannotBeEdited)
            {
                return "This CAB profile cannot be edited until it's been approved or declined.";
            }

            if (!EditByGroupPermitted && Status == Status.Draft && !IsPendingOgdApproval)
            {
                if (IsOpssAdmin || !IsUkas)
                {
                    if (RequestedFromCabProfilePage)
                    {
                        return "This CAB profile cannot be edited as a draft CAB profile has already been created by a UKAS user.";
                    }
                    else
                    {
                        return "This CAB profile cannot be edited as it was created by a UKAS user.";
                    }
                }
                else 
                {
                    if (RequestedFromCabProfilePage)
                    {
                        return "This CAB profile cannot be edited as a draft CAB profile has already been created by an OPSS user.";
                    }
                    else
                    {
                        return "This CAB profile cannot be edited as it was created by an OPSS user.";
                    }                        
                }
            }

            return string.Empty;
        }

        private bool ShowEditButtonForOgdOrOpssNonOwner =>
            !UserInCreatorUserGroup && 
            (ShowEditButtonForOpssNonOwner ||
            ShowEditButtonForOgdNonOwner);

        private bool ShowEditButtonForOpssNonOwner =>
            IsOpssAdmin &&
            (SubStatus == SubStatus.None ||
            (Status == Status.Draft &&
            SubStatus == SubStatus.PendingApprovalToPublish &&
            IsActionableByOpssAdmin));

        private bool ShowEditButtonForOgdNonOwner =>
            HasOgdRole &&
        Status == Status.Draft &&
        SubStatus == SubStatus.PendingApprovalToPublish &&
        LegislativeAreasPendingApprovalForCurrentUserCount > 0;

        private bool ShowEditButtonForUkasOrOpssOwner =>
            (IsUkas || IsOpssAdmin) &&
           ShowEditButtonForOwnerBase;

        private bool ShowEditButtonForOwnerBase =>
           UserInCreatorUserGroup &&
           (Status == Status.Draft || Status == Status.Published) &&
           SubStatus == SubStatus.None;

        private bool ShowSubSectionEditActionForOpss =>
            !UserInCreatorUserGroup &&
            Status == Status.Draft &&
            SubStatus != SubStatus.None &&
            HasActionableLegislativeAreaForOpssAdmin &&
            IsOpssAdmin;

        private bool ShowSubSectionEditActionForOwner =>
            UserInCreatorUserGroup &&
            Status == Status.Draft &&
            SubStatus == SubStatus.None;

        private bool ShowProfileVisibilityWarningForOpssOwner =>
                    IsOpssAdmin &&
                    UserInCreatorUserGroup &&
                    Status == Status.Draft;

        private bool ShowProfileVisibilityWarningForOpssNonOwner =>
            IsOpssAdmin &&
            !UserInCreatorUserGroup &&
            Status == Status.Draft &&
            HasActionableLegislativeAreaForOpssAdmin;
    }
}
