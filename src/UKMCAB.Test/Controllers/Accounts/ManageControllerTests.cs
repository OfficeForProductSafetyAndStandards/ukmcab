using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UKMCAB.Common.Exceptions;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI;
using UKMCAB.Web.UI.Areas.Account.Controllers;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Test.Controllers.Accounts
{
    public class ManageControllerTests
    {
        private ManageController _sut;
        private Mock<IAsyncNotificationClient> mockAsyncNotificationClient;
        private Mock<IOptions<TemplateOptions>> mockTemplateOptions = new();

        private Mock<UserManager<UKMCABUser>> mockUserManager;
        private Mock<SignInManager<UKMCABUser>> mockSignInManager;

        private TemplateOptions _templateOptions = new TemplateOptions
        {
            RegistrationRequest = "register", RegisterRequestConfirmation = "register-confirmation",
            RegistrationApproved = "registration-approved", RegistrationRejected = "registration-rejected"
        };

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<UKMCABUser>>();

            mockUserManager =
                new Mock<UserManager<UKMCABUser>>(store.Object, null, null, null, null, null, null, null, null);
            mockSignInManager = new Mock<SignInManager<UKMCABUser>>(
                mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<UKMCABUser>>().Object,
                null,
                null,
                null,
                null);
            ;
            mockTemplateOptions.Setup(to => to.Value).Returns(_templateOptions);
            mockAsyncNotificationClient = new();

            _sut = new ManageController(mockUserManager.Object, mockSignInManager.Object, mockAsyncNotificationClient.Object,
                mockTemplateOptions.Object);
        }

        [Test]
        public async Task ChangePasswordPageThrowsErrorIfNoAuthenticatedUserLoggedIn()
        {
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((UKMCABUser)null);

            Assert.ThrowsAsync<PermissionDeniedException>(() => _sut.ChangePassword());
        }

        [Test]
        public async Task ChangePasswordFormSubmissionThrowsErrorIfNoAuthenticatedUserLoggedIn()
        {
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((UKMCABUser)null);

            Assert.ThrowsAsync<PermissionDeniedException>(() => _sut.ChangePassword(new ChangePasswordViewModel()));
        }

        [Test]
        public async Task ChangePasswordFormSubmissionShowsErrorIfPasswordChangeFails()
        {
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UKMCABUser());
            mockUserManager
                .Setup(um => um.ChangePasswordAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Password", Description = "Failed" }));

            var result = await _sut.ChangePassword(new ChangePasswordViewModel()) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task ChangePasswordFormSubmissionSendsEmailReSignsInAndRedirectsOnSuccess()
        {
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new UKMCABUser{Email = "test@email.com"});
            mockUserManager
                .Setup(um => um.ChangePasswordAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _sut.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            var result = await _sut.ChangePassword(new ChangePasswordViewModel()) as RedirectToActionResult;

            mockSignInManager.Verify(sim => sim.RefreshSignInAsync(It.IsAny<UKMCABUser>()), Times.Once);

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            Assert.IsTrue(_sut.TempData["Email"] == "test@email.com");
            Assert.IsTrue(result.ActionName == nameof(ManageController.PasswordChanged));
        }

        [Test]
        public async Task PendingRequestsReturnsValidRequests()
        {
            mockUserManager.Setup(um => um.GetUsersInRoleAsync(Constants.Roles.OGDUser)).ReturnsAsync(
                new List<UKMCABUser>
                {
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = true, Email = "ogdUser1@beis.gov.uk" },
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = false, Email = "ogdUser2@beis.gov.uk" },
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = false, Email = "ogdUser3@beis.gov.uk" },
                });
            mockUserManager.Setup(um => um.GetUsersInRoleAsync(Constants.Roles.OPSSAdmin)).ReturnsAsync(
                new List<UKMCABUser>
                {
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = true, Email = "opssUser1@beis.gov.uk" },
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = true, Email = "opssUser2@beis.gov.uk" },
                    new UKMCABUser { EmailConfirmed = true, RequestApproved = false, Email = "opssUser3@beis.gov.uk" },
                });

            var result = await _sut.PendingRequests() as ViewResult;

            var model = result.Model as PendingAccountsViewModel;

            Assert.IsNotNull(model);
            Assert.IsTrue(model.PendingUsers.Count == 3);
        }

        [Test]
        public async Task RequestReviewThrowsNotFoundWhenNoId()
        {
            Assert.ThrowsAsync<NotFoundException>(() => _sut.RequestReview((string)null));
        }

        [Test]
        public async Task RequestReviewThrowsNotFoundWhenNoUserFound()
        {
            var id = "123";
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync((UKMCABUser)null);
            Assert.ThrowsAsync<NotFoundException>(() => _sut.RequestReview(id));
        }

        [Test]
        public async Task RequestReviewThrowsDomainExceptionWhenUserAlreadyConfirmed()
        {
            var id = "123";
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(new UKMCABUser {EmailConfirmed = false});
            Assert.ThrowsAsync<DomainException>(() => _sut.RequestReview(id));
        }

        [Test]
        public async Task RequestReviewViewModelWithUserForValidId()
        {
            var id = "123";
            var user = new UKMCABUser { EmailConfirmed = true, Email = "test@email.com" };
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);

            var result = await _sut.RequestReview(id) as ViewResult;

            var model = result.Model as RequestReviewViewModel;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.UserForReview == user);
        }

        [Test]
        public async Task SubmitApprovalOrRejectionRequestReviewThrowsNotFoundWhenNoUserFound()
        {
            var id = "123";
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync((UKMCABUser)null);

            Assert.ThrowsAsync<NotFoundException>(() => _sut.RequestReview(new RequestReviewViewModel(), id, "Approve"));
            Assert.ThrowsAsync<NotFoundException>(() => _sut.RequestReview(new RequestReviewViewModel(), id, "Reject"));
        }

        [Test]
        public async Task SubmitApprovalOrRejectionRequestReviewThrowsDomainExceptionWhenUserAlreadyConfirmed()
        {
            var id = "123";
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(new UKMCABUser { EmailConfirmed = false });

            Assert.ThrowsAsync<DomainException>(() => _sut.RequestReview(new RequestReviewViewModel(), id, "Approve"));
            Assert.ThrowsAsync<DomainException>(() => _sut.RequestReview(new RequestReviewViewModel(), id, "Reject"));
        }

        [Test]
        public async Task SubmitApprovalRequestReturnsInvalidModelStateIfUserNotUpdated()
        {
            var id = "123";
            var user = new UKMCABUser { EmailConfirmed = true, Email = "test@email.com", RequestApproved = false};
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Update", Description = "Failed" }));
            
            var result = await _sut.RequestReview(new RequestReviewViewModel(), id, "Approve") as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task SubmitApprovalRequestSendsEmailAndRedirectsIfUserUpdated()
        {
            var id = "123";
            var user = new UKMCABUser { EmailConfirmed = true, Email = "test@email.com", RequestApproved = false };
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            TestHelper.SetupUrlActionReturn(_sut);

            var result = await _sut.RequestReview(new RequestReviewViewModel(), id, "Approve") as RedirectToActionResult;

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(user.Email, _templateOptions.RegistrationApproved,
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()));

            Assert.IsTrue(result.ActionName == nameof(ManageController.PendingRequests));
        }

        [Test]
        public async Task SubmitRejectionRequestReturnsInvalidModelStateIfUserNotUpdated()
        {
            var id = "123";
            var user = new UKMCABUser { EmailConfirmed = true, Email = "test@email.com", RequestApproved = false };
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Update", Description = "Failed" }));

            var result = await _sut.RequestReview(new RequestReviewViewModel(), id, "Reject") as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task SubmitRejectionRequestSendsEmailAndRedirectsIfUserUpdated()
        {
            var id = "123";
            var user = new UKMCABUser { EmailConfirmed = true, Email = "test@email.com", RequestApproved = false };
            mockUserManager.Setup(um => um.FindByIdAsync(id)).ReturnsAsync(user);
            mockUserManager.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);
            TestHelper.SetupUrlActionReturn(_sut);

            var result = await _sut.RequestReview(new RequestReviewViewModel(), id, "Reject") as RedirectToActionResult;

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(user.Email, _templateOptions.RegistrationRejected,
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()));

            Assert.IsTrue(result.ActionName == nameof(ManageController.PendingRequests));
        }
    }
}
