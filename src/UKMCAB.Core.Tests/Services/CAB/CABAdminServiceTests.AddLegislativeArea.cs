using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UKMCAB.Data.Models.Users;

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
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.AddLegislativeAreaAsync(Guid.NewGuid(), Guid.NewGuid(), "Lifts"), "No document found");
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
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.AddLegislativeAreaAsync(Guid.NewGuid(), laId, "test"));
            return Task.CompletedTask;
        }

        [Test]
        public async Task NewLegislativeId_AddLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            const string cabId = "cabId";
            var legislativeArea = new Mock<DocumentLegislativeArea>();
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId,
                        DocumentLegislativeAreas = new()
                        {
                            legislativeArea.Object
                        }
                    }
                });

            // Act
            await _sut.AddLegislativeAreaAsync(Guid.Parse(cabId), legislativeArea.Object.LegislativeAreaId, "La to Add");

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId && d.DocumentLegislativeAreas.Contains(legislativeArea.Object))),
                Times.Once);
            _mockCABRepository.VerifyNoOtherCalls();
        }
    }
}