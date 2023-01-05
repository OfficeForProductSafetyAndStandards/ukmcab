using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Areas.Admin.Controllers;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Test.Controllers.Admin
{
    public class CABControllerTests
    {
        private CABController _sut;
        private Mock<UserManager<UKMCABUser>> mockUserManager;
        private ICABAdminService _CABAdminService;
        private Mock<ICABRepository> _ICABRepository = new();

        [SetUp]
        public void Setup()
        {
            var store = new Mock<IUserStore<UKMCABUser>>();
            mockUserManager =
                new Mock<UserManager<UKMCABUser>>(store.Object, null, null, null, null, null, null, null, null);

            _CABAdminService = new CABAdminService(_ICABRepository.Object);

            _sut = new CABController(_CABAdminService, mockUserManager.Object);
        }

        [Test]
        public async Task CreateCABWithoutRegulationsCausesError()
        {
            var result = await _sut.Create(new CreateCABViewModel
            {
                Name = "TEST",
                Address = "test",
                Website = "test",
                Regulations = new List<string> ()
            }) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task CreateCABWithoutContactDetailCausesError()
        {
            var result = await _sut.Create(new CreateCABViewModel
            {
                Name = "TEST",
                Address = "test",
                Regulations = new List<string> {"test"}
            }) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task CreateCABWithExistingNameCausesError()
        {
            _ICABRepository.Setup(r => r.Query(It.IsAny<string>())).ReturnsAsync(new List<Document> { new Document() });

            var result = await _sut.Create(new CreateCABViewModel
            {
                Name = "TEST",
                Address = "test",
                Website = "test.com",
                Regulations = new List<string> { "test" }
            }) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }

        [Test]
        public async Task CreateCABWithUniqueNameSucceeds()
        {
            _ICABRepository.Setup(r => r.Query(It.IsAny<string>())).ReturnsAsync(new List<Document>());

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new UKMCABUser { Email = "test@test.com" });

            _ICABRepository.Setup(r => r.CreateAsync(It.IsAny<Document>())).ReturnsAsync(new Document());

            var result = await _sut.Create(new CreateCABViewModel
            {
                Name = "TEST",
                Address = "test",
                Website = "test.com",
                Regulations = new List<string> { "test" }
            }) as RedirectResult;

            Assert.IsTrue(result.Url == "/");
        }

        [Test]
        public async Task CreateCABWithUniqueNameNotSavedReturnsError()
        {
            _ICABRepository.Setup(r => r.Query(It.IsAny<string>())).ReturnsAsync(new List<Document>());

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new UKMCABUser { Email = "test@test.com" });

            _ICABRepository.Setup(r => r.CreateAsync(It.IsAny<Document>())).ReturnsAsync(default(Document));

            var result = await _sut.Create(new CreateCABViewModel
            {
                Name = "TEST",
                Address = "test",
                Website = "test.com",
                Regulations = new List<string> { "test" }
            }) as ViewResult;

            Assert.IsFalse(result.ViewData.ModelState.IsValid);
        }
    }
}
