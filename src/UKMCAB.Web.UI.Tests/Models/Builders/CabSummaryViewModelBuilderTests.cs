using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.Builders;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

#pragma warning disable CS8618

namespace UKMCAB.Web.UI.Tests.Models.Builders
{
    [TestFixture]
    public class CabSummaryViewModelBuilderTests
    {
        private Mock<ICabLegislativeAreasItemViewModelBuilder> _mockCabLegislativeAreasItemViewModelBuilder;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<HttpContext> _mockHttpContext;

        private CabSummaryViewModelBuilder _sut;

        [SetUp]
        public void Setup()
        {
            _mockCabLegislativeAreasItemViewModelBuilder = new Mock<ICabLegislativeAreasItemViewModelBuilder>(MockBehavior.Strict);
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            _mockHttpContext = new Mock<HttpContext>();

            SetupUser(Roles.OPSS.Id);
        }

        [Test]
        public void WithDocumentDetails_PopulatesDocumentDetails()
        {
            // Arrange
            var document = new Document
            {
                id = "Test document id",
                CABId = "Test cab id",
                StatusValue = Status.Unknown,
                SubStatus = SubStatus.None,
                LastUpdatedDate = new DateTime(2024, 1, 1)
            };

            var expectedResult = new CABSummaryViewModel
            {
                Id = "Test document id",
                CABId = "Test cab id",
                Status = Status.Unknown,
                SubStatus = SubStatus.None,
                SubStatusName = "None",
                StatusCssStyle = string.Empty,
                HasActiveLAs = false,
                LastModifiedDate = new DateTime(2024, 1, 1),
                IsPendingOgdApproval = false,
                LegislativeAreasApprovedByAdminCount = 0,
                LegislativeAreaHasBeenActioned = false,
                IsActionableByOpssAdmin = true,
                DraftUpdated = true
            };

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithDocumentDetails(document).Build();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithLegislativeAreasPendingApprovalCount_UserIsNotOpssAdmin_PopulatesLegislativeAreasPendingApprovalCountWithOgdPendingApprovalCount()
        {
            // Arrange
            var document = new Document
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        RoleId = Roles.DFTP.Id,
                        Status = LAStatus.PendingApproval
                    }
                }
            };

            SetupUser(Roles.DFTP.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithLegislativeAreasPendingApprovalCount(document).Build();

            // Assert
            result.LegislativeAreasPendingApprovalForCurrentUserCount.Should().Be(1);
        }

        [Test]
        public void WithLegislativeAreasPendingApprovalCount_UserIsOpssAdmin_PopulatesLegislativeAreasPendingApprovalCountWithOpssAdminPendingApprovalCount()
        {
            // Arrange
            var document = new Document
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        RoleId = Roles.OPSS.Id,
                        Status = LAStatus.Approved
                    }
                }
            };

            SetupUser(Roles.OPSS.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithLegislativeAreasPendingApprovalCount(document).Build();

            // Assert
            result.LegislativeAreasPendingApprovalForCurrentUserCount.Should().Be(1);
        }

        [TestCase("/test", ExpectedResult = "/test")]
        [TestCase("/", ExpectedResult = "/?")]
        [TestCase("", ExpectedResult = "")]
        public string WithReturnUrl_PopulatesReturnUrl(string url)
        {
            // Arrange
            var encodedUrl = WebUtility.UrlEncode(url);

            // Act
            var result = _sut.WithReturnUrl(encodedUrl).Build();

            // Assert
            return result.ReturnUrl;
        }

        [Test]
        public void WithReturnUrl_UrlIsWhiteSpace_PopulatesReturnUrl()
        {
            // Act
            var result = _sut.WithReturnUrl("  ").Build();

            // Assert
            result.ReturnUrl.Should().Be("");
        }

        [Test]
        public void WithRoleInfo_UserIsOppssAdmin_PupulatesRoleInfo()
        {
            // Arrange
            var document = new Document
            {
                CreatedByUserGroup = "Test user group"
            };
            var expectedResult = new CABSummaryViewModel
            {
                UserInCreatorUserGroup = false,
                IsOpssAdmin = true,
                IsUkas = false,
                IsOPSSOrInCreatorUserGroup = true,
                HasOgdRole = false
            };

            SetupUser(Roles.OPSS.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRoleInfo(document).Build();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithRoleInfo_UserIsOgd_PupulatesRoleInfo()
        {
            // Arrange
            var document = new Document
            {
                CreatedByUserGroup = "Test user group"
            };
            var expectedResult = new CABSummaryViewModel
            {
                UserInCreatorUserGroup = false,
                IsOpssAdmin = false,
                IsUkas = false,
                IsOPSSOrInCreatorUserGroup = false,
                HasOgdRole = true
            };

            SetupUser(Roles.DFTP.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRoleInfo(document).Build();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithRoleInfo_UserIsUkas_PupulatesRoleInfo()
        {
            // Arrange
            var document = new Document
            {
                CreatedByUserGroup = "Test user group"
            };
            var expectedResult = new CABSummaryViewModel
            {
                UserInCreatorUserGroup = false,
                IsOpssAdmin = false,
                IsUkas = true,
                IsOPSSOrInCreatorUserGroup = false,
                HasOgdRole = false
            };

            SetupUser(Roles.UKAS.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRoleInfo(document).Build();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void WithRoleInfo_UserIsCreatorGroup_PupulatesRoleInfo()
        {
            // Arrange
            var document = new Document
            {
                CreatedByUserGroup = Roles.UKAS.Id
            };
            var expectedResult = new CABSummaryViewModel
            {
                UserInCreatorUserGroup = true,
                IsOpssAdmin = false,
                IsUkas = true,
                IsOPSSOrInCreatorUserGroup = true,
                HasOgdRole = false
            };

            SetupUser(Roles.UKAS.Id);

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRoleInfo(document).Build();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestCase(null, ExpectedResult = false)]
        [TestCase(false, ExpectedResult = false)]
        [TestCase(true, ExpectedResult = true)]
        public bool WithRevealEditActions_PopulatesRevealEditActions(bool? revealEditActions)
        {
            // Arrange
            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRevealEditActions(revealEditActions).Build();

            // Assert
            return result.RevealEditActions;
        }

        [TestCase(null, ExpectedResult = false)]
        [TestCase(false, ExpectedResult = false)]
        [TestCase(true, ExpectedResult = true)]
        public bool WithFromCabProfilePage_PopulatesFromCabProfilePage(bool? fromCabProfilePage)
        {
            // Arrange
            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithRequestedFromCabProfilePage(fromCabProfilePage).Build();

            // Assert
            return result.RequestedFromCabProfilePage;
        }

        [Test]
        public void WithSuccessBannerMessage_PopulatesSuccessBannerMessage()
        {
            // Arrange
            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithSuccessBannerMessage("Test message").Build();

            // Assert
            result.SuccessBannerMessage.Should().Be("Test message");
        }

        [Test]
        public void WithIsEditLocked_PopulatesIsEditLocked()
        {
            // Arrange
            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithIsEditLocked(true).Build();

            // Assert
            result.IsEditLocked.Should().BeTrue();
        }

        [Test]
        public void WithCabDetails_PopulatesCabDetails()
        {
            // Arrange
            var model = new CABDetailsViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabDetails(model).Build();

            // Assert
            result.CabDetailsViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabContactViewModel_PopulatesCabContactViewModel()
        {
            // Arrange
            var model = new CABContactViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabContactViewModel(model).Build();

            // Assert
            result.CabContactViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCABBodyDetailsViewModel_PopulatesCABBodyDetailsViewModel()
        {
            // Arrange
            var model = new CABBodyDetailsMRAViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabBodyDetailsMRAViewModel(model).Build();

            // Assert
            result.CabBodyDetailsMRAViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabLegislativeAreasViewModel_PopulatesCabLegislativeAreasViewModel()
        {
            // Arrange
            var model = new CABLegislativeAreasViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabLegislativeAreasViewModel(model).Build();

            // Assert
            result.CabLegislativeAreasViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabProductScheduleDetailsViewModel_PopulatesCabProductScheduleDetailsViewModel()
        {
            // Arrange
            var model = new CABProductScheduleDetailsViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithProductScheduleDetailsViewModel(model).Build();

            // Assert
            result.CABProductScheduleDetailsViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabSupportingDocumentDetailsViewModel_PopulatesCabSupportingDocumentDetailsViewModel()
        {
            // Arrange
            var model = new CABSupportingDocumentDetailsViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabSupportingDocumentDetailsViewModel(model).Build();

            // Assert
            result.CABSupportingDocumentDetailsViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabGovernmentUserNotesViewModel_PopulatesCabGovernmentUserNotesViewModel()
        {
            // Arrange
            var model = new CABGovernmentUserNotesViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabGovernmentUserNotesViewModel(model).Build();

            // Assert
            result.CABGovernmentUserNotesViewModel.Should().NotBeNull();
        }

        [Test]
        public void WithCabHistoryViewModel_PopulatesCabHistoryViewModel()
        {
            // Arrange
            var model = new CABHistoryViewModel();

            _sut = new CabSummaryViewModelBuilder(_mockCabLegislativeAreasItemViewModelBuilder.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = _sut.WithCabHistoryViewModel(model).Build();

            // Assert
            result.CABHistoryViewModel.Should().NotBeNull();
        }

        private void SetupUser(string roleId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, roleId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpContextAccessor.SetupGet(m => m.HttpContext).Returns(_mockHttpContext.Object);
            _mockHttpContext.SetupGet(m => m.User).Returns(user);
        }
    }
}
