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
            //var doc = new Document();
            //doc.DocumentLegislativeAreas.Add(new DocumentLegislativeArea {Status = LAStatus.PendingApproval });

            cabSummary = new()
            {
                Status = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                //CabDetailsViewModel = new CABDetailsViewModel { IsCompleted = true },
                //CabContactViewModel = new CABContactViewModel { IsCompleted = true},
                //CabBodyDetailsViewModel = new CABBodyDetailsViewModel {  IsCompleted = true},
                //CabLegislativeAreasViewModel = new CABLegislativeAreasViewModel { IsCompleted= true},
                //CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel {  IsCompleted= true},
                //CABSupportingDocumentDetailsViewModel   = new CABSupportingDocumentDetailsViewModel {  IsCompleted= true},
                //CABHistoryViewModel = new CABHistoryViewModel { IsCompleted = true},
                //CABGovernmentUserNotesViewModel = new CABGovernmentUserNotesViewModel {  IsCompleted= true},
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

        //[Category("CAB Summary page - Banner Content Happy Path")]
        //[Test]
        //public void GetBannerContent_Should_Return_CorrectString_When_OPSSAdminUserAccessesAUKASDraftCABFromCABProfilePage()
        //{
        //    // Arrange
        //    var _sut = new CABSummaryViewModel
        //    {
        //        EditByGroupPermitted = false,
        //        Status = Status.Draft,
        //        IsPendingOgdApproval = false,
        //        IsOpssAdmin = true,
        //        RequestedFromCabProfilePage = true,
        //    };

        //    var expectedString = "This CAB profile cannot be edited as a draft CAB profile has already been created by a UKAS user.";

        //    //Act
        //    var result = _sut.GetBannerContent();

        //    //Assert
        //    Assert.That(result, Is.EqualTo(expectedString));
        //}

        //[Category("CAB Summary page - Banner Content Happy Path")]
        //[Test]
        //public void GetBannerContent_Should_Return_CorrectString_When_OPSSAdminUserAccessesAUKASDraftCABNotFromCABProfilePage()
        //{
        //    // Arrange
        //    var _sut = new CABSummaryViewModel
        //    {
        //        EditByGroupPermitted = false,
        //        Status = Status.Draft,
        //        IsPendingOgdApproval = false,
        //        IsOpssAdmin = true,
        //        RequestedFromCabProfilePage = false,
        //    };

        //    var expectedString = "This CAB profile cannot be edited as it was created by a UKAS user.";

        //    //Act
        //    var result = _sut.GetBannerContent();

        //    //Assert
        //    Assert.That(result, Is.EqualTo(expectedString));
        //}

        //[Category("CAB Summary page - Banner Content Happy Path")]
        //[Test]
        //public void GetBannerContent_Should_Return_CorrectString_When_AUkasUserTriesToCreateADraftCABFromCABProfilePageOfACABWithAnExistingDraftCreatedByOPSSAdmin()
        //{
        //    // Arrange
        //    var _sut = new CABSummaryViewModel
        //    {
        //        EditByGroupPermitted = false,
        //        Status = Status.Draft,
        //        IsPendingOgdApproval = false,
        //        IsOpssAdmin = false,
        //        IsUkas = true,
        //        RequestedFromCabProfilePage = true,
        //    };

        //    var expectedString = "This CAB profile cannot be edited as a draft CAB profile has already been created by an OPSS user.";

        //    //Act
        //    var result = _sut.GetBannerContent();

        //    //Assert
        //    Assert.That(result, Is.EqualTo(expectedString));
        //}

        //[Category("CAB Summary page - Banner Content Happy Path")]
        //[Test]
        //public void GetBannerContent_Should_Return_CorrectString_When_OtherUsersAccessesADraftCABCreatedByOPSSAdmin()
        //{
        //    // Arrange
        //    var _sut = new CABSummaryViewModel
        //    {
        //        EditByGroupPermitted = false,
        //        IsPendingOgdApproval = false,
        //    };

        //    var expectedString = "This CAB profile cannot be edited as it was created by an OPSS user.";

        //    //Act
        //    var result = _sut.GetBannerContent();

        //    //Assert
        //    Assert.That(result, Is.EqualTo(expectedString));
        //}

        //[Category("CAB Summary page - Banner Content Happy Path")]
        //[Test]
        //public void GetBannerContent_Should_Return_CorrectString_When_AUserAccessesADraftCAB_ThatIsCurrentlyOpenedByAnotherUser()
        //{
        //    // Arrange
        //    var _sut = new CABSummaryViewModel
        //    {
        //        IsEditLocked = true,
        //        IsPendingOgdApproval = false,
        //        EditByGroupPermitted = true
        //    };

        //    var expectedString = "This CAB profile cannot be edited as it's being edited by another user.";

        //    //Act
        //    var result = _sut.GetBannerContent();

        //    //Assert
        //    Assert.That(result, Is.EqualTo(expectedString));
        //}

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
        [TestCase(true, false, false, true, false,false, SubStatus.None, false, true)]
        [TestCase(false, true, false, true, false,false, SubStatus.None, false, true)]
        [TestCase(true, false, false, false, false,false, SubStatus.PendingApprovalToPublish, true, true)]
        [TestCase(false, false, true, false, false, false, SubStatus.None, false, false)]
        public void ShowEditButton_Should_Return_CorrectValue_ForOpssAdmin(bool isOpssAdmin, bool isUkas, bool isOgd, bool inUserGroup, bool revealEditActions, bool isEditLoced, SubStatus subStatus, bool hasActionableLAForOpss, bool expectedResult)
        {
            // Arrange            
            cabSummary.IsOpssAdmin = isOpssAdmin;
            cabSummary.IsUkas = isUkas;
            cabSummary.HasOgdRole = isOgd;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;
            cabSummary.SubStatus = subStatus;
            cabSummary.HasActionableLegislativeAreaForOpssAdmin = hasActionableLAForOpss;

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

        [Category("CAB Summary page - Show Edit Button")]
        [Test, TestCaseSource(typeof(CABSummaryViewModelTestsHelpers), nameof(CABSummaryViewModelTestsHelpers.GetTestCases))]
        public void ShowEditButton_Should_Return_CorrectValue_For_OpssAdmin(bool isOpss, bool inUserGroup, Status status, SubStatus substatus, bool isActionable, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange
            cabSummary.IsOpssAdmin = isOpss;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
            cabSummary.Status = status;
            cabSummary.SubStatus = substatus;
            cabSummary.HasActionableLegislativeAreaForOpssAdmin = isActionable;
            cabSummary.RevealEditActions = revealEditActions;
            cabSummary.IsEditLocked = isEditLoced;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Review Button")]
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

        [Category("CAB Summary page - Show Review Button")]
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
