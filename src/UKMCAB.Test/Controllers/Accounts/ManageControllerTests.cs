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
                mockTemplateOptions.Object, null);
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
            var result = await _sut.ChangePassword(new ChangePasswordViewModel()) as ViewResult;

            mockSignInManager.Verify(sim => sim.RefreshSignInAsync(It.IsAny<UKMCABUser>()), Times.Once);

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            var model = result.Model as ChangePasswordViewModel;
            Assert.IsTrue(model.PasswordChanged);
        }
    }
}
