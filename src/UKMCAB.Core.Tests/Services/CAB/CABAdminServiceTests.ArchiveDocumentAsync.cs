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
using EntityFrameworkCore.Testing.Moq.Extensions;
using UKMCAB.Core.Tests.Extensions;

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

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            var result = await _sut.ArchiveDocumentAsync(new Mock<UserAccount>().Object, document.CABId, "test internal reason", "test publicreason");

            // Assert
            Assert.That(Enum.GetNames(typeof(LAStatus)).Length - 1, Is.EqualTo(result.DocumentLegislativeAreas.Count));
            Assert.That(result.DocumentLegislativeAreas.All(la => la.Archived!.Value), Is.True);
            Assert.That(Status.Archived, Is.EqualTo(result.StatusValue));
        }
    }
}