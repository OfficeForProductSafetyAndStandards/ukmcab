using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UKMCAB.Core.Security;
using UKMCAB.Core.Security.Requirements;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Tests.Security.Requirements
{
    [TestFixture]
    public class DeleteCabHandlerTests
    {
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<ICABAdminService> _mockCABAdminService;
        private DeleteCabHandler _handlerUnderTest;

        public DeleteCabHandlerTests()
        { 
            _mockContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            _mockCABAdminService = new Mock<ICABAdminService>(MockBehavior.Strict);
            _handlerUnderTest = new DeleteCabHandler(_mockCABAdminService.Object, _mockContextAccessor.Object);
        }

        [Test]
        public async Task Should_Throw_Exception_When_Cab_Not_Found()
        { 
            //Arrange 
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(new DefaultHttpContext());

            var authorizationContext = new AuthorizationHandlerContext(new[] { new DeleteCabRequirement() }, new System.Security.Claims.ClaimsPrincipal(), null);

            //Act
            //Assert
            Assert.ThrowsAsync<Exception>(() => _handlerUnderTest.HandleAsync(authorizationContext)); 
        }

        [Test]
        public async Task Should_Fail_When_Status_Is_Not_Draft()
        {
            //Arrange 
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary() { { "cabId", "a-test-cab-id-000" } };
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(mockContext);

            _mockCABAdminService.Setup(s => s.GetLatestDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new Document());

            var authorizationContext = new AuthorizationHandlerContext(
                new[] { new DeleteCabRequirement() },
                new ClaimsPrincipal(),
                null);

            //Act
            await _handlerUnderTest.HandleAsync(authorizationContext);

            //Assert 
            Assert.IsFalse(authorizationContext.HasSucceeded);
        }


        [Test]
        public async Task Should_Fail_When_SubStatus_Is_Not_None()
        {
            //Arrange 
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary() { { "cabId", "a-test-cab-id-000" } };
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(mockContext);

            _mockCABAdminService.Setup(s => s.GetLatestDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new Document()
                {
                    StatusValue = Status.Draft,
                    SubStatus = SubStatus.PendingApprovalToUnpublish
                });

            var authorizationContext = new AuthorizationHandlerContext(
                new[] { new DeleteCabRequirement() },
                new ClaimsPrincipal(),
                null);

            //Act
            await _handlerUnderTest.HandleAsync(authorizationContext);

            //Assert 
            Assert.IsFalse(authorizationContext.HasSucceeded);
        }

        [Theory]
        [TestCaseSource(typeof(RolesData), nameof(RolesData.All))]
        public async Task Should_Succeed_When_SubStatus_Is_None_And_User_Is_OPSSAdmin(string createdByRole)
        {
            //Arrange 
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary() { { "cabId", "a-test-cab-id-000" } };
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(mockContext);

            _mockCABAdminService.Setup(s => s.GetLatestDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new Document()
                {
                    StatusValue = Status.Draft,
                    CreatedByUserGroup = createdByRole
                });
            
            //OPSS Admin user:
            var authorizationContext = new AuthorizationHandlerContext(
                new[] { new DeleteCabRequirement() },
                GenerateMockPrincipal(Roles.OPSS.Id),
                null);

            //Act
            await _handlerUnderTest.HandleAsync(authorizationContext);

            //Assert 
            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        [Theory]
        [TestCaseSource(typeof(RolesData), nameof(RolesData.All))]
        public async Task Should_Succeed_When_SubStatus_Is_None_And_Is_Created_By_User_In__Role(string createdByRole)
        {
            //Arrange 
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary() { { "cabId", "a-test-cab-id-000" } };
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(mockContext);

            _mockCABAdminService.Setup(s => s.GetLatestDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new Document()
                {
                    StatusValue = Status.Draft,
                    CreatedByUserGroup = createdByRole
                });

            var authorizationContext = new AuthorizationHandlerContext(
                new[] { new DeleteCabRequirement() },
                GenerateMockPrincipal(createdByRole),
                null);

            //Act
            await _handlerUnderTest.HandleAsync(authorizationContext);

            //Assert 
            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        [Test]
        public async Task Should_Succeed_When_SubStatus_Is_None_And_User_Is_Ukas()
        {
            //Arrange 
            var mockContext = new DefaultHttpContext();
            mockContext.Request.RouteValues = new RouteValueDictionary() { { "cabId", "a-test-cab-id-000" } };
            _mockContextAccessor.Setup(m => m.HttpContext).Returns(mockContext);

            _mockCABAdminService.Setup(s => s.GetLatestDocumentAsync(It.IsAny<string>()))
                .ReturnsAsync(new Document()
                { 
                    StatusValue = Status.Draft,
                    CreatedByUserGroup = Roles.UKAS.Id
                });

            var authorizationContext = new AuthorizationHandlerContext(
                new[] { new DeleteCabRequirement() },
                GenerateMockPrincipal(Roles.UKAS.Id),
                null);

            //Act
            await _handlerUnderTest.HandleAsync(authorizationContext);

            //Assert 
            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        private ClaimsPrincipal GenerateMockPrincipal(string roleId, params Claim[] claims)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, roleId) });
            foreach (var claim in claims)
            {
                identity.AddClaim(claim);
            }
            return new ClaimsPrincipal(identity);
        }

        private class RolesData 
        {
            public static IEnumerable<string> All
            {
                get
                {
                    return Roles.List.Select(r => r.Id);
                }
            }
        }
    }
}
