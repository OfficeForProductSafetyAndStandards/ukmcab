using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Security;

namespace UKMCAB.Core.Tests.Extensions
{
    [TestFixture]
    public class ClaimsPrincipalExtensionsTests
    {
        [TestCaseSource(nameof(HasOgdRoleTestData))]
        public void HasOgdRole_RoleInOgdRolesList_ReturnsTrue(string role)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, role),
            };
            var sut = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = sut.HasOgdRole();

            result.Should().BeTrue();
        }

        [Test]
        public void HasOgdRole_RoleNotInOgdRolesList_ReturnsFalse()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, "Test role"),
            };
            var sut = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = sut.HasOgdRole();

            result.Should().BeFalse();
        }

        private static IEnumerable<string> HasOgdRoleTestData
        {
            get
            {
                foreach (var role in Roles.OgdRolesList)
                {
                    yield return role;
                }
            }
        }
    }
}
