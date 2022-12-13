using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using Notify.Models.Responses;
using UKMCAB.Common.Exceptions;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Areas.Account.Controllers;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Test.Controllers.Accounts
{
    public class ForgotPasswordControllerTests
    {
        private ForgotPasswordController _sut;
        private Mock<IAsyncNotificationClient> mockAsyncNotificationClient;
        private Mock<UserManager<UKMCABUser>> mockUserManager = new();
        private Mock<IOptions<TemplateOptions>> mockTemplateOptions = new();


        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<UKMCABUser>>();
            mockUserManager =
                new Mock<UserManager<UKMCABUser>>(store.Object, null, null, null, null, null, null, null, null);
            mockTemplateOptions.Setup(to => to.Value).Returns(new TemplateOptions { ResetPassword = "reset-password" });
            mockAsyncNotificationClient = new();
            _sut = new ForgotPasswordController(mockAsyncNotificationClient.Object, mockUserManager.Object,
                mockTemplateOptions.Object);
        }

        [Test]
        public async Task RedirectToConfirmationWhenUserNotFound()
        {
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);

            var result =
                await _sut.Index(new ForgotPasswordViewModel { Email = "abc@abc.com" }) as RedirectToActionResult;

            Assert.IsTrue(result.ActionName == nameof(ForgotPasswordController.Confirmation));
        }

        [Test]
        public async Task RedirectToConfirmationWhenUserEmailNotConfirmed()
        {
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UKMCABUser
                { EmailConfirmed = false, Email = "abc@abc.com" });

            var result =
                await _sut.Index(new ForgotPasswordViewModel { Email = "abc@abc.com" }) as RedirectToActionResult;

            Assert.IsTrue(result.ActionName == nameof(ForgotPasswordController.Confirmation));
        }

        [Test]
        public async Task RedirectToConfirmationAndEmailSentWhenUserConfirmed()
        {
            // Arrange
            var user = new UKMCABUser { EmailConfirmed = true, Email = "abc@abc.com" };
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUserManager.Setup(um => um.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            mockUserManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<UKMCABUser>()))
                .ReturnsAsync("abcd");
            mockAsyncNotificationClient
                .Setup(asm => asm.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmailNotificationResponse());

            TestHelper.SetupUrlActionReturn(_sut);

            // Act
            var result =
                await _sut.Index(new ForgotPasswordViewModel { Email = "abc@abc.com" }) as RedirectToActionResult;

            //Assert
            //Email event fired
            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()));
            // Redirect correct
            Assert.IsTrue(result.ActionName == nameof(ForgotPasswordController.Confirmation));
        }

        [Test]
        public async Task ResetWithoutCodeFails()
        {
            Assert.Throws<NotFoundException>(() => _sut.Reset((string)null));
        }

        [Test]
        public async Task RedirectToResetPasswordConfirmationWhenUserNotFound()
        {
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);

            var result =
                await _sut.Reset(new ResetPasswordViewModel{Email = "abc@abc.com"}) as RedirectToActionResult;

            Assert.IsTrue(result.ActionName == nameof(ForgotPasswordController.ResetPasswordConfirmation));
        }

        [Test]
        public async Task RedirectToResetPasswordConfirmationAndEmailSentWhenUserPasswordReset()
        {
            // Arrange
            var user = new UKMCABUser { EmailConfirmed = true, Email = "abc@abc.com" };
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            mockUserManager.Setup(um => um.ResetPasswordAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mockAsyncNotificationClient
                .Setup(asm => asm.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmailNotificationResponse());
            

            // Act
            var result =
                await _sut.Reset(new ResetPasswordViewModel { Email = "abc@abc.com", Password = "Password", Code = "123"}) as RedirectToActionResult;

            //Assert
            //Email event fired
            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            // Redirect correct
            Assert.IsTrue(result.ActionName == nameof(ForgotPasswordController.ResetPasswordConfirmation));
        }

        [Test]
        public async Task NoRedirectAndNoEmailSentWhenUserPasswordFailsToReset()
        {
            // Arrange
            var user = new UKMCABUser { EmailConfirmed = true, Email = "abc@abc.com" };
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            mockUserManager.Setup(um => um.ResetPasswordAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError{Code = "An", Description = "Error"}));
            mockAsyncNotificationClient
                .Setup(asm => asm.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new EmailNotificationResponse());

            // Act
            var result =
                await _sut.Reset(new ResetPasswordViewModel { Email = "abc@abc.com", Password = "Password", Code = "123" }) as ViewResult;

            //Assert
            //Email event not fired
            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            // Errors recorded
            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

    }
}
