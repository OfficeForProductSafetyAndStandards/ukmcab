using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using System.Linq;
using UKMCAB.Core.Security;
using NUnit.Framework.Legacy;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task ArchiveDocumentAsync_AllLegislativeAreasArchived()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var document = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                CreatedByUserGroup = Roles.OPSS.Id,
                StatusValue = Status.Published,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules
            };

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document> { document });

            // Act
            var result = await _sut.ArchiveDocumentAsync(new Mock<UserAccount>().Object, document.CABId, "test internal reason", "test publicreason");

            // ClassicAssert
            ClassicAssert.AreEqual(Enum.GetNames(typeof(LAStatus)).Length - 1, result.DocumentLegislativeAreas.Count);
            ClassicAssert.True(result.DocumentLegislativeAreas.All(la => la.Archived!.Value));
            ClassicAssert.AreEqual(Status.Archived, result.StatusValue);
        }
    }
}