using NUnit.Framework;
using System;
using System.Collections.Generic;
using UKMCAB.Core.Extensions;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.Builders;
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
                CabDetailsViewModel = new CABDetailsViewModel { IsCompleted = true },
                CabContactViewModel = new CABContactViewModel { IsCompleted = true},
                CabBodyDetailsViewModel = new CABBodyDetailsViewModel {  IsCompleted = true},
                CabLegislativeAreasViewModel = new CABLegislativeAreasViewModel { IsCompleted= true},
                CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel {  IsCompleted= true},
                CABSupportingDocumentDetailsViewModel   = new CABSupportingDocumentDetailsViewModel {  IsCompleted= true},
                CABHistoryViewModel = new CABHistoryViewModel { IsCompleted = true},
                CABGovernmentUserNotesViewModel = new CABGovernmentUserNotesViewModel {  IsCompleted= true},
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
            cabSummary.IsOpssAdmin = true;
            cabSummary.DraftUpdated = true;
            cabSummary.IsPendingOgdApproval = false;
            cabSummary.HasActiveLAs = true; 
            cabSummary.DraftUpdated = true; 

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
        [TestCase(true, true, false,false, true)]
        [TestCase(true, false, false, false, false)]
        public void ShowEditButton_Should_Return_CorrectValue_ForOpssAdmin(bool isOpssAdmin, bool inUserGroup, bool revealEditActions, bool isEditLoced, bool expectedResult)
        {
            // Arrange            
            cabSummary.IsOpssAdmin = isOpssAdmin;
            cabSummary.UserInCreatorUserGroup = inUserGroup;
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
            cabSummary.LegislativeAreasPendingApprovalCount = laPendingApproval;

            //Act
            var result = cabSummary.ShowEditButton;

            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Category("CAB Summary page - Show Edit Button")]
        [Test, TestCaseSource(nameof(GetTestCases))]
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
        public static IEnumerable<TestCaseData> GetTestCases()
        {
            var doc = new Document();
            var docLA = doc.DocumentLegislativeAreas;
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Approved });

            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true);
            
            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Declined });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclined_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToRemoveByOPSS });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToRemoveByOPSS_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsApprovedByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToRemoveByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToRemoveByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToRemoveByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToRemoveByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToArchiveAndArchiveScheduleByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToArchiveAndRemoveScheduleByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndArchiveScheduleByOpssAdmin_ShouldBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndRemoveScheduleByOpssAdmin");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndArchiveScheduleByOGD");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndArchiveScheduleByOPSS");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndRemoveScheduleByOGD");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndRemoveScheduleByOPSS");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToUnarchiveByOPSS });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToUnarchiveByOPSS");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToUnarchiveByOpssAdmin });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToUnarchiveByOpssAdmin");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToUnarchiveByOPSS });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToUnarchiveByOPSS");


            // Sad Path
            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApproval });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingApproval_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Draft });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsDraft_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToUnarchive });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToUnarchive_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndArchiveSchedule });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndArchiveSchedule_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.None });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsNone_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToRemove });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToRemove_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingSubmissionToArchiveAndArchiveSchedule });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingSubmissionToArchiveAndArchiveSchedule_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingSubmissionToArchiveAndRemoveSchedule });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingSubmissionToArchiveAndRemoveSchedule_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingSubmissionToRemove });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingSubmissionToRemove_ShouldNotBeActionable");

            docLA.Clear();
            docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingSubmissionToUnarchive });
            yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.HasActionableLegislativeAreaForOpssAdmin(), false, false, false).SetName("Test_WhenLAStatusIsPendingSubmissionToUnarchive_ShouldNotBeActionable");
        }
    }
}
