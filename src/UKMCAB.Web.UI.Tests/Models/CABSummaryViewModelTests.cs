using NUnit.Framework;
using System;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.Tests.Models
{

    [TestFixture]
    public class CABSummaryViewModelTests
    {
        private CABSummaryViewModel cabSummary;

        [SetUp]
        public void SetUp()
        {
            cabSummary = new()
            {
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                IsPendingOgdApproval = true,
                RevealEditActions = true,
                IsEditLocked = false,
                PublishedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
            };            
        }

        #region Test - Banner Content 

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_EmptyString_When_OPSSAdminIsReviewingACabWithAllLaRequests_ActionedByOPSS()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                IsOpssAdmin = true,
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                HasActionableLegislativeAreaForOpssAdmin = true,
                IsActionableByOpssAdmin = true,
            };

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(""));
        }
        
        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_EmptyString_When_OpssAdminIsReviewingAnOpssActionableCabThatWasSubmittedForOgdApproval()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                IsOpssAdmin = true,
                UserInCreatorUserGroup = true,
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                IsActionableByOpssAdmin = true,
            };

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_ExpectedString_When_OPSSAdminIsReviewingACabWithAtLeastOneLaRequest_ActionedByOgd_AndAtLeastOnePendingOgdApproval()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                IsOpssAdmin = true,
                SubStatus = SubStatus.PendingApprovalToPublish,
                IsPendingOgdApproval = true,
                HasActionableLegislativeAreaForOpssAdmin = true,
            };

            var expectedString = string.Empty;

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_CorrectString_When_OPSSAdminUserAccessesAUKASDraftCABFromCABProfilePage()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                Status = Status.Draft,
                IsPendingOgdApproval = false,
                IsOpssAdmin = true,
                RequestedFromCabProfilePage = true,
            };
            CABSummaryViewModelTestsHelpers.SetEditByGroupPermittedToFalse(_sut);

            var expectedString = "This CAB profile cannot be edited as a draft CAB profile has already been created by a UKAS user.";

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_CorrectString_When_OPSSAdminUserAccessesAUKASDraftCABNotFromCABProfilePage()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                IsPendingOgdApproval = false,
                IsOpssAdmin = true,
                RequestedFromCabProfilePage = false,
            };

            CABSummaryViewModelTestsHelpers.SetEditByGroupPermittedToFalse(_sut);

            var expectedString = "This CAB profile cannot be edited as it was created by a UKAS user.";

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_CorrectString_When_AUkasUserTriesToCreateADraftCABFromCABProfilePageOfACABWithAnExistingDraftCreatedByOPSSAdmin()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                Status = Status.Draft,
                IsPendingOgdApproval = false,
                IsOpssAdmin = false,
                IsUkas = true,
                RequestedFromCabProfilePage = true,
            };

            CABSummaryViewModelTestsHelpers.SetEditByGroupPermittedToFalse(_sut);

            var expectedString = "This CAB profile cannot be edited as a draft CAB profile has already been created by an OPSS user.";

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_CorrectString_When_OtherUsersAccessesADraftCABCreatedByOPSSAdmin()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                UserInCreatorUserGroup = false,
                IsPendingOgdApproval = false,
                IsUkas = true
            };

            CABSummaryViewModelTestsHelpers.SetEditByGroupPermittedToFalse(_sut);

            var expectedString = "This CAB profile cannot be edited as it was created by an OPSS user.";

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_CorrectString_When_AUserAccessesADraftCAB_ThatIsCurrentlyOpenedByAnotherUser()
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                IsEditLocked = true,
                IsPendingOgdApproval = false
            };

            CABSummaryViewModelTestsHelpers.SetEditByGroupPermittedToFalse(_sut);

            var expectedString = "This CAB profile cannot be edited as it's being edited by another user.";

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }

        #endregion


        #region Test - Show Profile Visibility Warning

        [Category("CAB Summary page - Show Profile Visibility Warning")]
        [Test]
        public void ShowProfileVisibilityWarning_Should_Return_True()
        {
            // Arrange
            CABSummaryViewModelTestsHelpers.SetValidCABToTrueOpssAdmin(cabSummary);
            CABSummaryViewModelTestsHelpers.SetCanPublishToTrueOpssAdmin(cabSummary);
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);

            //Act
            var result = cabSummary.ShowProfileVisibilityWarning;

            //Assert
            Assert.That(result, Is.True);
        }

        #endregion


        #region Test - Show Substatus Name

        [Category("CAB Summary page - Show Substatus Name")]
        [TestCase(SubStatus.PendingApprovalToPublish, "Pending approval to publish CAB", true)]
        [TestCase(SubStatus.None, "None", false)]
        public void ShowSubstatusName_Should_Return_True(SubStatus subStatus, string subStatusName, bool expectedResult)
        {
            // Arrange
            var _sut = new CABSummaryViewModel
            {
                SubStatus = subStatus,
                SubStatusName = subStatusName,
            };

            //Act
            var result = _sut.ShowSubstatusName;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        #endregion


        #region Test - Show Edit Button

        [Category("CAB Summary page - Show Edit Button")]
        [TestCase(true, false, true, Status.Draft, SubStatus.None, false, false, true)]
        [TestCase(false, true, true, Status.Draft, SubStatus.None, false, false, true)]
        public void ShowEditButton_Should_Return_CorrectValue_ForUkasAndOpssAdmin(bool isUkas, bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange
            cabSummary.IsUkas = isUkas;
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Edit Button")]
        [TestCase(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, false, false, true, 1, true)]
        [TestCase(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, false, false, true, 0, false)]
        public void ShowEditButton_Should_Return_CorrectValue_ForOgd(bool hasOgdRole, bool inUserGroup, Status status, SubStatus substatus, bool revealEditActions, bool isEditLoced, bool isPendingOgdApproval, int laPendingApproval, bool expectedResult)
        {
            // Arrange            
            cabSummary.HasOgdRole = hasOgdRole;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;
            cabSummary.IsPendingOgdApproval = isPendingOgdApproval;
            cabSummary.LegislativeAreasPendingApprovalForCurrentUserCount = laPendingApproval;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Edit Button for OPSS admin")]
        [Test, TestCaseSource(typeof(CABSummaryViewModelTestsHelpers), nameof(CABSummaryViewModelTestsHelpers.GetTestCases))]
        public void ShowEditButton_Should_Return_CorrectValue_For_OpssAdmin(bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool isActionableByOpssAdmin, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.IsActionableByOpssAdmin = isActionableByOpssAdmin;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Edit Button when OPSS admin submits cab for ogd's approval")]
        [TestCase(true, true, Status.Draft, SubStatus.PendingApprovalToPublish, true, false, false, true)]
        [TestCase(true, true, Status.Draft, SubStatus.PendingApprovalToPublish, false, false, false,false)]
        public void ShowEditButton_Should_Return_CorrectValue_When_OpssAdminSubmitsACabForOgdApproval(bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool isActionableByOpssAdmin, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.IsActionableByOpssAdmin = isActionableByOpssAdmin;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        #endregion

        #region Test - Show Subsection Edit action

        [Category("CAB Summary page - Show Subsection Edit action")]
        [TestCase(true, true, Status.Draft, SubStatus.PendingApprovalToPublish, true, true, false, true)]
        [TestCase(true, true, Status.Draft, SubStatus.PendingApprovalToPublish, false, true, false, false)]

        public void ShowSubsectionEditAction_Should_Return_CorrectValue_WhenOpssAdminSubmitsForOgdApproval(bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool isActionableByOpssAdmin, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange            
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.IsActionableByOpssAdmin = isActionableByOpssAdmin;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowSubSectionEditAction;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        #endregion


        #region Test - Show Review Button
        [Category("CAB Summary page - Show Review Button for OPSS")]
        [Test]
        public void ShowReviewButton_Should_Return_True_ForOpss()
        {
            // Arrange
            cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
            cabSummary.LegislativeAreasPendingApprovalForCurrentUserCount = 1;
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);

            // Act
            var result = cabSummary.ShowReviewButton;

            // Assert
            Assert.That(result, Is.True);
        }

        [Category("CAB Summary page - Show Review Button for OGD")]
        [Test]
        public void ShowReviewButton_Should_Return_True_ForOgd()
        {
            // Arrange
            cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
            CABSummaryViewModelTestsHelpers.SetShowOgdActionsToTrue(cabSummary);

            // Act
            var result = cabSummary.ShowReviewButton;

            // Assert
            Assert.That(result, Is.True);
        }

        [Category("CAB Summary page - Show Review Button")]
        [Test]
        public void ShowReviewButton_Should_Return_False_ForOgd_WhenSubstatusIsNone()
        {
            // Arrange
            cabSummary.SubStatus = SubStatus.None;
            CABSummaryViewModelTestsHelpers.SetShowOgdActionsToTrue(cabSummary);

            // Act
            var result = cabSummary.ShowReviewButton;

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion


        #region Test - Show Publish Button

        [Category("CAB Summary page - Show Publish Button")]
        [Test]
        public void ShowPublishButton_Should_Return_True_When_Opss_Is_The_Creator()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            cabSummary.IsOpssAdmin = true;
            cabSummary.UserInCreatorUserGroup = true;
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = SubStatus.None;
            cabSummary.DraftUpdated = true;

            cabSummary.IsEditLocked = false;

            // Act
            var result = cabSummary.ShowPublishButton;

            // Assert
            Assert.That(result, Is.True);
        }
        #endregion


        #region Test - Show Approve To Publish Button

        [Category("CAB Summary page - Show Approve To Publish Button")]
        [Test]        
        public void ShowApproveToPublishButton_Should_Return_True_When_OgdHasApproved()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            CABSummaryViewModelTestsHelpers.SetCanPublishToTrueOpssAdmin(cabSummary);
            cabSummary.UserInCreatorUserGroup = false;
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
            cabSummary.IsEditLocked = false;

            // Act
            var result = cabSummary.ShowApproveToPublishButton;

            // Assert
            Assert.That(result, Is.True);
        }

        [Category("CAB Summary page - Show Approve To Publish Button")]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ShowApproveToPublishButton_Should_Return_CorrectValue_When_OpssAdminSubmitsForOgdApproval(bool isActionableByOpssAdmin, bool expectedResult)
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            CABSummaryViewModelTestsHelpers.SetCanPublishToTrueOpssAdmin(cabSummary);
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
            cabSummary.IsActionableByOpssAdmin = isActionableByOpssAdmin;
            cabSummary.UserInCreatorUserGroup = true;

            // Act
            var result = cabSummary.ShowApproveToPublishButton;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        #endregion

        #region Test - Can submit for approval

        [Category("CAB Summary page - Can Submit For Approval action")]
        [TestCase(true, false, true, true)]
        [TestCase(false, true, true, true)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]

        public void CanSubmitForApproval_Should_Return_CorrectValue_WhenOpssAdminSubmitsForOgdApproval(bool isOpss, bool isUkas, bool draftUpdated, bool expectedResult)
        {
            // Arrange            
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.IsUkas = isUkas;
            cabSummary.DraftUpdated = draftUpdated;
            CABSummaryViewModelTestsHelpers.SetIsCompleteToTrue(cabSummary);


            //Act
            var result = cabSummary.CanSubmitForApproval;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }


        #endregion

        #region Test - Show Mandatory Info Warning

        [Category("CAB Summary page - Show Mandatory Info Warning")]
        [Test]
        public void ShowMandatoryInfoWarning_Should_Return_False()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetRevealEditTrueAndIsEditLockedFalse(cabSummary);
            CABSummaryViewModelTestsHelpers.SetValidCABToTrueOpssAdmin(cabSummary);
            cabSummary.IsOpssAdmin = true;

            // Act
            var result = cabSummary.ShowMandatoryInfoWarning;

            // Assert
            Assert.That(result, Is.False);
        }

        [Category("CAB Summary page - Show Mandatory Info Warning")]
        [Test]
        public void ShowMandatoryInfoWarning_Should_Return_True()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetRevealEditTrueAndIsEditLockedFalse(cabSummary);
            CABSummaryViewModelTestsHelpers.SetValidCABToFalseOpssAdmin(cabSummary);
            cabSummary.IsOpssAdmin = true;

            // Act
            var result = cabSummary.ShowMandatoryInfoWarning;

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion


        #region Test - Show Decline Button

        [Category("CAB Summary page - Show Decline Button")]
        [Test]
        public void ShowDeclineButton_Should_Return_True()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            CABSummaryViewModelTestsHelpers.SetCanPublishToTrueOpssAdmin(cabSummary);
            cabSummary.UserInCreatorUserGroup = false;
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
            cabSummary.IsEditLocked = false;

            // Act
            var result = cabSummary.ShowDeclineButton;

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion


        #region Test - Show Save as draft

        [Category("CAB Summary page - Show Save as draft Button")]
        [Test]
        public void ShowSaveAsDraftButton_Should_Return_True()
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = SubStatus.None;

            // Act
            var result = cabSummary.ShowSaveAsDraftButton;

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion


        #region Test - Show Delete Draft Button

        [Category("CAB Summary page - Show Delete Draft Button")]
        [TestCase(true, true, false, SubStatus.None, true)]
        [TestCase(true, false, true, SubStatus.None, false)]
        public void ShowDeleteDraftButton_Should_Return_True_ForOpssAdmin(bool revealEditActions, bool isOpss, bool isUkas, SubStatus subStatus, bool expectedResult)
        {
            // Arrange            
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = false;
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = subStatus;

            // Act
            var result = cabSummary.ShowDeleteDraftButton;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Delete Draft Button")]
        [TestCase(SubStatus.None, true)]
        [TestCase(SubStatus.PendingApprovalToPublish, false)]
        public void ShowDeleteDraftButton_Should_Return_True_WhenSaveAsDraftIsTrue(SubStatus subStatus, bool expectedResult)
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = subStatus;

            // Act
            var result = cabSummary.ShowDeleteDraftButton;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        #endregion


        #region Test - Show Cancel Publish Button

        [Category("CAB Summary page - Show Cancel Publish Button")]
        [TestCase(true, false, SubStatus.None, true)]
        [TestCase(false, true, SubStatus.None, true)]
        [TestCase(true, false, SubStatus.PendingApprovalToPublish, false)]
        [TestCase(false, true, SubStatus.PendingApprovalToPublish, false)]
        public void ShowCancelPublishButton_Should_Return_CorrectValueWhenShowPublishButtonIsTrue(bool isOpss, bool isUkas, SubStatus subStatus, bool expectedResult)
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueOpssAdmin(cabSummary);
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.IsUkas = isUkas;
            cabSummary.UserInCreatorUserGroup = true;
            cabSummary.Status = Status.Draft;
            cabSummary.SubStatus = subStatus;

            cabSummary.IsEditLocked = false;

            // Act
            var result = cabSummary.ShowCancelPublishButton;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Cancel Publish Button")]
        [TestCase(SubStatus.None, true)]
        [TestCase(SubStatus.PendingApprovalToPublish, false)]
        public void ShowCancelPublishButton_Should_Return_CorrectValueWhenShowSubmitForApprovalButtonIsTrue(SubStatus subStatus, bool expectedResult)
        {
            // Arrange            
            CABSummaryViewModelTestsHelpers.SetShowSubSectionEditActionToTrueForUkas(cabSummary);
            cabSummary.SubStatus = subStatus;


            // Act
            var result = cabSummary.ShowCancelPublishButton;

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        #endregion
    }
}
