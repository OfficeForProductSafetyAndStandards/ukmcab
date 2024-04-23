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

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task DocumentNotCreatedByOPSS_PublishDocumentAsync_LAsNotApprovedByOPSSAdminRemoved()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                new Document
                {
                    StatusValue = Status.Draft,
                    DocumentLegislativeAreas = legislativeAreas,
                    ScopeOfAppointments = scopeOfAppointments,
                    Schedules = schedules
                });

            // Assert
            Assert.AreEqual(3, result.DocumentLegislativeAreas.Count);
            Assert.AreEqual(LAStatus.Published, result.DocumentLegislativeAreas.First().Status);
            Assert.AreEqual(3, result.ScopeOfAppointments.Count);
            Assert.AreEqual(3, result.Schedules?.Count);
        }

        [Test]
        public async Task DocumentCreatedByOPSS_PublishDocumentAsync_AllLAsPublished()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                new Document
                {
                    CreatedByUserGroup = Roles.OPSS.Id,
                    StatusValue = Status.Draft,
                    DocumentLegislativeAreas = legislativeAreas,
                    ScopeOfAppointments = scopeOfAppointments,
                });

            // Assert
            Assert.AreEqual(Enum.GetNames(typeof(LAStatus)).Length - 1, result.DocumentLegislativeAreas.Count);
            Assert.True(result.DocumentLegislativeAreas.All(la => la.Status == LAStatus.Published));
        }

        private (List<DocumentLegislativeArea>, List<DocumentScopeOfAppointment>, List<FileUpload>) GenerateTestData()
        {
            var legislativeAreas = new List<DocumentLegislativeArea>();
            var scopeOfAppointments = new List<DocumentScopeOfAppointment>();
            var schedules = new List<FileUpload>();

            foreach (var status in Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s => s != LAStatus.Published))
            {
                var legislativeAreaId = Guid.NewGuid();
                legislativeAreas.Add(new DocumentLegislativeArea
                {
                    LegislativeAreaId = legislativeAreaId,
                    LegislativeAreaName = legislativeAreaId.ToString(),
                    Status = status,
                });
                scopeOfAppointments.Add(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                });
                schedules.Add(new FileUpload
                {
                    LegislativeArea = legislativeAreaId.ToString(),
                });
            }

            return (legislativeAreas, scopeOfAppointments, schedules);
        }
    }
}