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

        [Category("CAB Summary page - Banner Content Happy Path")]
        [Test]
        public void GetBannerContent_Should_Return_EmptyString_When_OPSSAdminIsReviewingACabWithAllLaRequests_ActionedByOPSS() 
        { 
            // Arrange
            var _sut = new CABSummaryViewModel{ 
                IsOpssAdmin = true,
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                HasActionableLegislativeAreaForOpssAdmin = true,
                LegislativeAreaHasBeenActioned = true,
            };

            //Act
            var result = _sut.GetBannerContent();

            //Assert
            Assert.That(result, Is.EqualTo(""));
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
        public void ShowEditButton_Should_Return_CorrectValue_For_OpssAdmin(bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool isPendingOgdApproval, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.IsPendingOgdApproval = isPendingOgdApproval;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

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

            cabSummary.IsEditLocked = false;

            // Act
            var result = cabSummary.ShowPublishButton;

            // Assert
            Assert.That(result, Is.True);
        }

        [Category("CAB Summary page - Show Publish Button")]
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
    }
}
