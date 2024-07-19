using NUnit.Framework;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;


namespace UKMCAB.Web.Tests.Models
{
    [TestFixture]
    public class CABSummaryViewModelTests
    {
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
    }
}
