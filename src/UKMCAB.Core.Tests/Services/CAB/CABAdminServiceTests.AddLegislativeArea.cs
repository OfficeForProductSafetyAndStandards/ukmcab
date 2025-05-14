using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Core.Security;
using UKMCAB.Core.Tests.Extensions;
using UKMCAB.Data.Models.LegislativeAreas;
using System.Linq;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public Task DocumentNotFound_AddLegislativeAreaAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.AddLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), Guid.NewGuid(), "Lifts","ogd", false), "No document found");
            return Task.CompletedTask;
        }

        [Test]
        public Task LegislativeIdExists_AddLegislativeAreaAsync_ThrowsException()
        {
            // Arrange
            var laId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        DocumentLegislativeAreas = new List<DocumentLegislativeArea>()
                        {
                            new()
                            {
                                LegislativeAreaId = laId
                            }
                        }
                    }
                });

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.AddLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), laId, "test","ogd", false), "Legislative id already exists on cab");
            return Task.CompletedTask;
        }

        [Test]
        public async Task NewLegislativeId_AddLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var legislativeArea = new Mock<DocumentLegislativeArea>();

            var document = new Document
            {
                CABId = cabId.ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = new()
                        {
                            legislativeArea.Object
                        }
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            await _sut.AddLegislativeAreaAsync(new Mock<UserAccount>().Object, cabId, Guid.NewGuid(), "La to Add","ogd", false);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && d.DocumentLegislativeAreas.Contains(legislativeArea.Object)), null),
                Times.Once);            
        }
    }
}