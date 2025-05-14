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
using UKMCAB.Core.Tests.Extensions;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public Task DocumentNotFound_UnarchiveDocumentAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.UnarchiveDocumentAsync(new Mock<UserAccount>().Object, _faker.Random.Guid().ToString(), _faker.Random.Words(10), _faker.Random.Words(10), true));
            return Task.CompletedTask;
        }

        [Test]
        public Task ArchivedDocNotFound_UnarchiveDocumentAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        StatusValue = Status.Published
                    },
                    new()
                    {
                        StatusValue = Status.Historical
                    }
                });

            // Act and Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.UnarchiveDocumentAsync(new Mock<UserAccount>().Object, _faker.Random.Guid().ToString(), _faker.Random.Words(10), _faker.Random.Words(10), true));
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.VerifyNoOtherCalls();
            return Task.CompletedTask;
        }


       
        [TestCase(true, LAStatus.Draft)]
        [TestCase(false, LAStatus.Published)]
        public async Task UnarchiveDocumentAsync_All_LegislativeAreas_Archived_And_Correct_Status(bool legislativeAreasAsDraft, LAStatus expectedLAStatus)
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var document = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                CreatedByUserGroup = Roles.OPSS.Id,
                StatusValue = Status.Archived,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>() {
                    new DocumentLegislativeArea {
                        Status = LAStatus.Published
                    },
                    new DocumentLegislativeArea {
                        Status = LAStatus.Published
                    },
                    new DocumentLegislativeArea {
                        Status = LAStatus.Published
                    },
                    new DocumentLegislativeArea {
                        Status = LAStatus.Published
                    },
                    new DocumentLegislativeArea {
                        Status = LAStatus.Published
                    },
                }
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));
            _mockCABRepository.Setup(x => x.CreateAsync(It.IsAny<Document>(), It.IsAny<DateTime>())).ReturnsAsync(document);

            // Act
            var result = await _sut.UnarchiveDocumentAsync(new Mock<UserAccount>().Object, document.CABId, 
                _faker.Random.Words(10), _faker.Random.Words(10), true, legislativeAreasAsDraft);

            // Assert
            Assert.That(document.DocumentLegislativeAreas.Count, Is.EqualTo(result.DocumentLegislativeAreas.Count));
            Assert.That(result.DocumentLegislativeAreas.All(la => !la.Archived!.Value), Is.True);
            Assert.That(result.DocumentLegislativeAreas.All(la => la.Status == expectedLAStatus), Is.True);
            Assert.That(Status.Draft, Is.EqualTo(result.StatusValue));
        }
    }
}