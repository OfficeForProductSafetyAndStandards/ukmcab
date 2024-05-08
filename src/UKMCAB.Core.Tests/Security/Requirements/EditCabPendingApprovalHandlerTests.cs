using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using UKMCAB.Core.Security.Requirements;
using UKMCAB.Core.Services.CAB;
using System.Security.Claims;
using UKMCAB.Core.Security;
using Microsoft.AspNetCore.Routing;
using UKMCAB.Data.Models;
using System.Collections.Generic;
using System;

namespace UKMCAB.Core.Tests.Security.Requirements
{
    public class EditCabPendingApprovalHandlerTests
    {
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<ICABAdminService> _mockCABAdminService;
        private EditCabPendingApprovalHandler _handler;

        public EditCabPendingApprovalHandlerTests() 
        {
            _mockContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            _mockCABAdminService = new Mock<ICABAdminService>(MockBehavior.Strict);
            _handler = new EditCabPendingApprovalHandler(_mockCABAdminService.Object, _mockContextAccessor.Object);
        }


        [Test]
        public async Task ShouldThrowException_When_IdNotFoundInRoute()
        {
            // Arrange
            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(new DefaultHttpContext());

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, new ClaimsPrincipal(), null);

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(authorizationContext));
        }

        [Test]
        public async Task ShoulSucceed_When_CabNotFound_And_UserIsOPSSAdmin()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync((Document?)null);
            
            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.OPSS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulSucceed_When_CabNotFound_And_UserIsUKAS()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync((Document?)null);

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.UKAS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulFail_When_CabNotFound_And_UserIsNotOPSSAdminOrUKAS()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync((Document?)null);

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.OPSS_OGD.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulSucceed_When_DocumentSubStatusIsPendingApprovalToPublish_And_UserIsOPSSAdmin_And_LegislativeAreaHasBeenActioned()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                SubStatus = SubStatus.PendingApprovalToPublish,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new DocumentLegislativeArea
                    {
                        Status = LAStatus.Approved,
                    }
                }
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.OPSS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulFail_When_DocumentSubStatusIsPendingApprovalToPublish_And_UserIsNotOPSSAdmin_And_LegislativeAreaHasBeenActioned()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                SubStatus = SubStatus.PendingApprovalToPublish,
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.UKAS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulFail_When_DocumentSubStatusIsPendingApprovalToPublish_And_UserIsOPSSAdmin_And_LegislativeAreaHasNotBeenActioned()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                SubStatus = SubStatus.PendingApprovalToPublish,
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.OPSS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulSucceed_When_DocumentSubStatusIsNotPendingApprovalToPublish_And_UserIsOPSSAdmin()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                SubStatus = SubStatus.None,
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.OPSS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulSucceed_When_DocumentSubStatusIsNotPendingApprovalToPublish_And_UserIsInCreatorUserGroup()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                CreatedByUserGroup = Roles.UKAS.Id,
                SubStatus = SubStatus.None,
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.UKAS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.True(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task ShoulFail_When_DocumentSubStatusIsNotPendingApprovalToPublish_And_UserIsNotOPSSAdmin_And_NotInCreatorUserGroup()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary { { "id", "test-cab-id" } };

            _mockContextAccessor.SetupGet(m => m.HttpContext).Returns(mockContext);
            _mockCABAdminService.Setup(m => m.GetLatestDocumentAsync(It.IsAny<string>())).ReturnsAsync(new Document
            {
                SubStatus = SubStatus.None,
            });

            var authorizationContext = new AuthorizationHandlerContext(new[] { new EditCabPendingApprovalRequirement() }, GenerateMockPrincipal(Roles.UKAS.Id), null);

            // Act
            await _handler.HandleAsync(authorizationContext);

            // Assert
            Assert.False(authorizationContext.HasSucceeded);
        }

        private ClaimsPrincipal GenerateMockPrincipal(string roleId, params Claim[] claims)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, roleId) });
            foreach(var claim in claims)
            {
                identity.AddClaim(claim);
            }
            return new ClaimsPrincipal(identity);
        }
    }
}
