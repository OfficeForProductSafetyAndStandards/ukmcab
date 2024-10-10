using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Services;
using UKMCAB.Data.Models;
using System;
using System.Collections.Generic;
using UKMCAB.Core.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using UKMCAB.Data.Models.Users;
using System.Linq;
using FluentAssertions;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

#pragma warning disable CS8618

namespace UKMCAB.Web.UI.Tests.Services
{
    [TestFixture]
    public class CabSummaryUiServiceTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ICABAdminService> _mockCabAdminService;
        private Mock<IEditLockService> _mockEditLockService;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<ITempDataDictionaryFactory> _mockTempDataDictionaryFactory;
        private Mock<ITempDataProvider> _mockTempDataProvider;
        private Mock<HttpContext> _mockHttpContext;

        private CabSummaryUiService _sut;
        private string userId;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>(MockBehavior.Strict);
            _mockCabAdminService = new Mock<ICABAdminService>(MockBehavior.Strict);
            _mockEditLockService = new Mock<IEditLockService>(MockBehavior.Strict);
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            _mockTempDataDictionaryFactory = new Mock<ITempDataDictionaryFactory>(MockBehavior.Strict);
            _mockTempDataProvider = new Mock<ITempDataProvider>();

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContextAccessor.SetupGet(m => m.HttpContext).Returns(_mockHttpContext.Object);

            userId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
            };
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockHttpContext.SetupGet(m => m.User).Returns(mockUser);
            _sut = new CabSummaryUiService(
                _mockUserService.Object,
                _mockCabAdminService.Object,
                _mockEditLockService.Object,
                _mockHttpContextAccessor.Object,
                _mockTempDataDictionaryFactory.Object);
        }

        [Test]
        public async Task CreateDocumentAsync_DocumentStatusIsPublishedAndSubSectionEditIsAllowed_CreatesNewDocument()
        {
            // Arrange
            var document = new Document
            {
                id = "Test id",
                StatusValue = Status.Published
            };
            var userAccount = new UserAccount
            {
                Id = "Test id",
            };

            _mockUserService.Setup(m => m.GetAsync(userId)).ReturnsAsync(userAccount);
            _mockCabAdminService.Setup(m => m.CreateDocumentAsync(
                It.Is<UserAccount>(u => u.Id == userAccount.Id), 
                It.Is<Document>(d => d.id == document.id))).ReturnsAsync(new Document());

            // Act
            await _sut.CreateDocumentAsync(document, true);

            // ClassicAssert
            _mockCabAdminService.Verify(m => m.CreateDocumentAsync(
                It.Is<UserAccount>(u => u.Id == userAccount.Id),
                It.Is<Document>(d => d.id == document.id)), Times.Once);
        }

        [TestCaseSource(nameof(CreateDocumentAsyncShouldNotCreateNewDocumentData))]
        public async Task CreateDocumentAsync_DocumentStatusNotPublishedOrSubSectionEditNotAllowed_DoesNotCreateNewDocument(Status status, bool subSectionEditAllowed)
        {
            // Arrange
            var document = new Document
            {
                StatusValue = status
            };

            // Act
            await _sut.CreateDocumentAsync(document, subSectionEditAllowed);

            // ClassicAssert
            _mockCabAdminService.Verify(m => m.CreateDocumentAsync(It.IsAny<UserAccount>(), It.IsAny<Document>()), Times.Never);
        } 

        [TestCase(Constants.ApprovedLA, "Legislative area has been approved.")]
        [TestCase(Constants.DeclinedLA, "Legislative area has been declined.")]
        public void GetSuccessBannerMessage_TempDataContainsKey_ReturnsMessageAndRemoveKeyFromTempData(string key, string expectedResult)
        {
            // Arrange
            var tempData = new TempDataDictionary(_mockHttpContext.Object, _mockTempDataProvider.Object)
            {
                [key] = true
            };

            _mockTempDataDictionaryFactory.Setup(m => m.GetTempData(It.IsAny<HttpContext>())).Returns(tempData);

            // Act
            var result = _sut.GetSuccessBannerMessage();

            // ClassicAssert
            result.Should().Be(expectedResult);
            tempData.ContainsKey(key).Should().BeFalse();
        }

        [Test]
        public void GetSuccessBannerMessage_TempDataDoesNotContainsKey_ReturnsNull()
        {
            // Arrange
            var tempData = new TempDataDictionary(_mockHttpContext.Object, _mockTempDataProvider.Object);

            _mockTempDataDictionaryFactory.Setup(m => m.GetTempData(It.IsAny<HttpContext>())).Returns(tempData);

            // Act
            var result = _sut.GetSuccessBannerMessage();

            // ClassicAssert
            result.Should().BeNull();
        }

        [TestCaseSource(nameof(LockCabForUserShouldLockCabForUserData))]
        public void LockCabForUser_SubSectionEditAllowedAndDocumentStatusIsDraftOrPublishedAndUserIsOPSSOrInCreatorUserGroup_LocksCabForUser(
            CABSummaryViewModel model)
        {
            // Arrange
            _mockEditLockService.Setup(m => m.SetAsync(model.CABId, userId));

            // Act
            var result = _sut.LockCabForUser(model);

            // ClassicAssert
            _mockEditLockService.Verify(m => m.SetAsync(model.CABId, userId), Times.Once);
        }

        [TestCaseSource(nameof(LockCabForUserShouldNotLockCabForUserData))]
        public async Task LockCabForUser_SubSectionEditNotAllowedOrDocumentStatusIsNotDraftOrPublishedOrUserIsNotOPSSOrInCreatorUserGroup_DoesNotLockCabForUser(
            CABSummaryViewModel model)
        {
            // Act
            await _sut.LockCabForUser(model);

            // ClassicAssert
            _mockEditLockService.Verify(m => m.SetAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static IEnumerable<TestCaseData> CreateDocumentAsyncShouldNotCreateNewDocumentData
        {
            get
            {
                foreach (var status in Enum.GetValues(typeof(Status)).Cast<Status>().Where(s => s != Status.Published))
                {
                    yield return new TestCaseData(status, true);
                }
                yield return new TestCaseData(Status.Published, false);
            }
        }

        private static IEnumerable<TestCaseData> LockCabForUserShouldLockCabForUserData
        {
            get
            {
                yield return new TestCaseData(new CABSummaryViewModel
                {
                    CABId = Guid.NewGuid().ToString(),
                    RevealEditActions = true,
                    Status = Status.Draft,
                    IsOPSSOrInCreatorUserGroup = true
                });
                yield return new TestCaseData(new CABSummaryViewModel
                {
                    CABId = Guid.NewGuid().ToString(),
                    RevealEditActions = true,
                    Status = Status.Published,
                    IsOPSSOrInCreatorUserGroup = true
                });
            }
        }

        private static IEnumerable<TestCaseData> LockCabForUserShouldNotLockCabForUserData
        {
            get
            {
                foreach (var status in Enum.GetValues(typeof(Status)).Cast<Status>().Where(s => s is not Status.Draft and not Status.Published))
                {
                    yield return new TestCaseData(new CABSummaryViewModel
                    {
                        CABId = Guid.NewGuid().ToString(),
                        RevealEditActions = true,
                        Status = status,
                        IsOPSSOrInCreatorUserGroup = true
                    });
                }
                yield return new TestCaseData(new CABSummaryViewModel
                {
                    CABId = Guid.NewGuid().ToString(),
                    RevealEditActions = false,
                    Status = Status.Draft,
                    IsOPSSOrInCreatorUserGroup = true
                });
                yield return new TestCaseData(new CABSummaryViewModel
                {
                    CABId = Guid.NewGuid().ToString(),
                    RevealEditActions = true,
                    Status = Status.Draft,
                    IsOPSSOrInCreatorUserGroup = false
                });
            }
        }
    }
}
