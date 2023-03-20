//using System.Linq.Expressions;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using System.Security.Claims;
//using UKMCAB.Core.Models;
//using UKMCAB.Core.Services;
//using UKMCAB.Identity.Stores.CosmosDB;
//using UKMCAB.Web.UI.Areas.Admin.Controllers;
//using UKMCAB.Web.UI.Models.ViewModels.Admin;

//namespace UKMCAB.Test.Controllers.Admin
//{
//    public class CABControllerTests
//    {
//        private CABController _sut;
//        private Mock<UserManager<UKMCABUser>> mockUserManager;
//        private ICABAdminService _CABAdminService;
//        private Mock<ICABRepository> _ICABRepository = new();
//        private Mock<IFileStorage> _filestorage = new();

//        [SetUp]
//        public void Setup()
//        {
//            var store = new Mock<IUserStore<UKMCABUser>>();
//            mockUserManager =
//                new Mock<UserManager<UKMCABUser>>(store.Object, null, null, null, null, null, null, null, null);
//            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
//                .ReturnsAsync(new UKMCABUser { Email = "test@test.com" });
//            _CABAdminService = new CABAdminService(_ICABRepository.Object);

//            _sut = new CABController(_CABAdminService, mockUserManager.Object);

//        }

//        [Test]
//        public async Task OPSSUserCreateCABWithoutRegulationsCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(false);

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                Address = "test",
//                Website = "test",
//                Regulations = new List<string> ()
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task UKASSUserCreateCABWithoutRegulationsCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(true);
//            var ukasReference = "1234";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                    d.CABData.UKASReference.Equals(ukasReference, StringComparison.CurrentCultureIgnoreCase)))
//                .ReturnsAsync(new List<Document>());

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                Address = "test",
//                Website = "test",
//                UKASReference = ukasReference,
//                Regulations = new List<string>()
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task UKASSUserCreateCABWithoutUKASReferenceNumberCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(true);

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                Address = "test",
//                Website = "test",
//                UKASReference = string.Empty,
//                Regulations = new List<string> { "Test" }
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task UKASUserCreateCABWithExistingUKASReferenceCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(true);
//            var ukasReference = "1234";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                    d.CABData.UKASReference.Equals(ukasReference, StringComparison.CurrentCultureIgnoreCase)))
//                .ReturnsAsync(new List<Document> { new Document() });

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                UKASReference = ukasReference,
//                Address = "test",
//                Website = "test",
//                Regulations = new List<string> {"Test"}
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task OPSSCreateCABWithoutContactDetailCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(false);
//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                Address = "test",
//                Regulations = new List<string> {"test"}
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task UKASCreateCABWithoutContactDetailCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(true);
//            var ukasReference = "1234";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                    d.CABData.UKASReference.Equals(ukasReference, StringComparison.CurrentCultureIgnoreCase)))
//                .ReturnsAsync(new List<Document>() );
//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = "TEST",
//                UKASReference = ukasReference,
//                Address = "test",
//                Regulations = new List<string> { "test" }
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task CreateCABWithExistingNameCausesError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(false);
//            var name = "TEST";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                d.CABData.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))).ReturnsAsync(new List<Document> { new Document() });

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = name,
//                Address = "test",
//                Website = "test.com",
//                Regulations = new List<string> { "test" }
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }

//        [Test]
//        public async Task OPSSCreateCABWithUniqueNameSucceeds()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(false);
//            var name = "TEST";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                d.CABData.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))).ReturnsAsync(new List<Document>());

//            var cabId = Guid.NewGuid().ToString();
//            _ICABRepository.Setup(r => r.CreateAsync(It.IsAny<Document>())).ReturnsAsync(new Document
//            {
//                CABData = new CABData
//                {
//                    CABId = cabId
//                }
//            });

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = name,
//                Address = "test",
//                Website = "test.com",
//                Regulations = new List<string> { "test" }
//            }) as RedirectToActionResult; 
//            Assert.IsTrue(result != null);
//            Assert.IsTrue(result.ActionName.Equals("SchedulesUpload"));
//            Assert.IsTrue(result.ControllerName.Equals("FileUpload"));
//            Assert.IsTrue(result.RouteValues["id"].Equals(cabId));
//        }

//        [Test]
//        public async Task CreateCABWithUniqueNameNotSavedReturnsError()
//        {
//            mockUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<UKMCABUser>(), It.IsAny<string>()))
//                .ReturnsAsync(false);
//            var name = "TEST";
//            _ICABRepository.Setup(r => r.Query<Document>(d =>
//                d.CABData.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))).ReturnsAsync(new List<Document>());

//            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
//                .ReturnsAsync(new UKMCABUser { Email = "test@test.com" });

//            _ICABRepository.Setup(r => r.CreateAsync(It.IsAny<Document>())).ReturnsAsync(default(Document));

//            var result = await _sut.Create(State.Saved, new CreateCABViewModel
//            {
//                Name = name,
//                Address = "test",
//                Website = "test.com",
//                Regulations = new List<string> { "test" }
//            }) as ViewResult;

//            Assert.IsFalse(result.ViewData.ModelState.IsValid);
//        }
//    }
//}
