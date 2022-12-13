using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Notify.Interfaces;
using UKMCAB.Core.Models.Account;
using UKMCAB.Core.Services.Account;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI;
using UKMCAB.Web.UI.Areas.Account.Controllers;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Test.Controllers.Accounts
{
    public class RegisterControllerTests
    {
        private RegisterController _sut;
        private Mock<IAsyncNotificationClient> mockAsyncNotificationClient;
        private Mock<UserManager<UKMCABUser>> mockUserManager = new();
        private Mock<IPasswordValidator<UKMCABUser>> mockPasswordValidator;
        private Mock<IOptions<TemplateOptions>> mockTemplateOptions = new();
        private Mock<IRegisterService> _mockRegisterService = new();

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<UKMCABUser>>();

            mockPasswordValidator = new Mock<IPasswordValidator<UKMCABUser>>();
            var mockPasswordValidatorList = new List<IPasswordValidator<UKMCABUser>> { mockPasswordValidator.Object };

            mockUserManager =
                new Mock<UserManager<UKMCABUser>>(store.Object, null, null, null, mockPasswordValidatorList, null, null, null, null);
            mockTemplateOptions.Setup(to => to.Value).Returns(new TemplateOptions { Register = "register", RegisterConfirmation = "register-confirmation"});
            mockAsyncNotificationClient = new();

            _sut = new RegisterController(_mockRegisterService.Object, mockAsyncNotificationClient.Object,
                mockTemplateOptions.Object, mockUserManager.Object);
        }

        [Test]
        public async Task NonUKASUserRegistrationWithNoLegislativeAreasCausesModelStateError()
        {
            var result =
                await _sut.RegisterRequest(new RegisterRequestViewModel { LegislativeAreas = new List<string>(), RequestReason = "reason"},
                    Constants.Roles.OGDUser) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task NonUKASUserRegistrationWithNoRequestReasonCausesModelStateError()
        {
            var result =
                await _sut.RegisterRequest(new RegisterRequestViewModel { LegislativeAreas = new List<string> {"Construction"}, RequestReason = "" },
                    Constants.Roles.OGDUser) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task RegistrationWithExistingUserCausesModelStateError()
        {
            var user = new UKMCABUser { EmailConfirmed = true, Email = "abc@abc.com" };
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var result =
                await _sut.RegisterRequest(new RegisterRequestViewModel { LegislativeAreas = new List<string> { "Construction" }, RequestReason = "reason" },
                    Constants.Roles.OGDUser) as ViewResult;

            mockUserManager.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task RegistrationWithIncorrectPasswordCausesModelStateError()
        {
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockPasswordValidator
                .Setup(p => p.ValidateAsync(It.IsAny<UserManager<UKMCABUser>>(), It.IsAny<UKMCABUser>(),
                    It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Password", Description = "Failed" }));


            var result =
                await _sut.RegisterRequest(new RegisterRequestViewModel { LegislativeAreas = new List<string> { "Construction" }, RequestReason = "reason" },
                    Constants.Roles.OGDUser) as ViewResult;

            mockUserManager.Verify(um => um.FindByEmailAsync(It.IsAny<string>()), Times.Once);

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task RegistrationWithValidPasswordResultsInEmailAndRedirect()
        {
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockPasswordValidator
                .Setup(p => p.ValidateAsync(It.IsAny<UserManager<UKMCABUser>>(), It.IsAny<UKMCABUser>(),
                    It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _mockRegisterService.Setup(rs => rs.EncodeRegistrationDetails(It.IsAny<RegistrationDTO>()))
                .Returns("encoded-string");

            TestHelper.SetupUrlActionReturn(_sut);

            var result =
                await _sut.RegisterRequest(new RegisterRequestViewModel { LegislativeAreas = new List<string> { "Construction" }, RequestReason = "reason" },
                    Constants.Roles.OGDUser) as RedirectToActionResult;

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()));
            // Redirect correct
            Assert.IsTrue(result.ActionName == nameof(RegisterController.RegisterConfirmation));
        }

        [Test]
        public async Task ConfirmRegistrationWithExistingEmailResultsInErrorMessage()
        {
            _mockRegisterService.Setup(rs => rs.DecodeRegistrationDetails("test-payload"))
                .Returns(new RegistrationDTO { Email = "test@email.com" });
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new UKMCABUser());

            var result = await _sut.ConfirmEmail("test-payload") as ViewResult;

            Assert.IsTrue(result.Model is ConfirmEmailViewModel);
            Assert.IsTrue(((ConfirmEmailViewModel)result.Model).Message == "There has been a problem with your registration details. Please try registering again or contact an administrator.");
        }

        [Test]
        public async Task ConfirmRegistrationUserCreationFailsResultsInErrorMessage()
        {
            _mockRegisterService.Setup(rs => rs.DecodeRegistrationDetails("test-payload"))
                .Returns(new RegistrationDTO { Email = "test@email.com", LegislativeAreas = new List<string>(), Password = "Password", Reason = "reason", UserRole = Constants.Roles.UKASUser});
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockUserManager.Setup(um => um.CreateAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Code = "User", Description = "Create error" }));
            mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<UKMCABUser>(), Constants.Roles.UKASUser)).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Code = "Role", Description = "Create error" }));

            var result = await _sut.ConfirmEmail("test-payload") as ViewResult;

            Assert.IsTrue(result.Model is ConfirmEmailViewModel);
            Assert.IsTrue(((ConfirmEmailViewModel)result.Model).Message == "There has been a problem registering your account. Please try again or contact an administrator.");
        }

        [Test]
        public async Task ConfirmRegistrationRoleLinkingFailsResultsInErrorMessage()
        {
            _mockRegisterService.Setup(rs => rs.DecodeRegistrationDetails("test-payload"))
                .Returns(new RegistrationDTO { Email = "test@email.com", LegislativeAreas = new List<string>(), Password = "Password", Reason = "reason", UserRole = Constants.Roles.UKASUser });
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockUserManager.Setup(um => um.CreateAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Success);
            mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<UKMCABUser>(), Constants.Roles.UKASUser)).ReturnsAsync(
                IdentityResult.Failed(new IdentityError { Code = "Role", Description = "Create error" }));

            var result = await _sut.ConfirmEmail("test-payload") as ViewResult;

            Assert.IsTrue(result.Model is ConfirmEmailViewModel);
            Assert.IsTrue(((ConfirmEmailViewModel)result.Model).Message == "There has been a problem registering your account. Please try again or contact an administrator.");
        }

        [Test]
        public async Task ConfirmRegistrationUKASUserResultsInSuccessMessage()
        {
            _mockRegisterService.Setup(rs => rs.DecodeRegistrationDetails("test-payload"))
                .Returns(new RegistrationDTO { Email = "test@email.com", LegislativeAreas = new List<string>(), Password = "Password", Reason = "reason", UserRole = Constants.Roles.UKASUser });
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockUserManager.Setup(um => um.CreateAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Success);
            mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<UKMCABUser>(), Constants.Roles.UKASUser)).ReturnsAsync(
                IdentityResult.Success);

            var result = await _sut.ConfirmEmail("test-payload") as ViewResult;

            Assert.IsTrue(result.Model is ConfirmEmailViewModel);
            Assert.IsTrue(((ConfirmEmailViewModel)result.Model).Message == "You will now be able to login to your account.");
        }

        [Test]
        public async Task ConfirmRegistrationNonUKASUserResultsInSuccessMessage()
        {
            _mockRegisterService.Setup(rs => rs.DecodeRegistrationDetails("test-payload"))
                .Returns(new RegistrationDTO { Email = "test@email.com", LegislativeAreas = new List<string>(), Password = "Password", Reason = "reason", UserRole = Constants.Roles.OGDUser });
            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((UKMCABUser)null);
            mockUserManager.Setup(um => um.CreateAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>())).ReturnsAsync(
                IdentityResult.Success);
            mockUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<UKMCABUser>(), Constants.Roles.OGDUser)).ReturnsAsync(
                IdentityResult.Success);

            var result = await _sut.ConfirmEmail("test-payload") as ViewResult;

            mockAsyncNotificationClient.Verify(anc => anc.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>()));

            Assert.IsTrue(result.Model is ConfirmEmailViewModel);
            Assert.IsTrue(((ConfirmEmailViewModel)result.Model).Message == "Your registration request will be reviewed and you will receive notification once approved.");
        }
    }
}
