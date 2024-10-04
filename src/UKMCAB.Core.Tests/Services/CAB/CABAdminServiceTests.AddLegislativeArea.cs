using NUnit.Framework;
using NUnit.Framework.Legacy;
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

            // Act and ClassicAssert
            ClassicAssert.ThrowsAsync<InvalidOperationException>(async () =>
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

            // Act and ClassicAssert
            ClassicAssert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.AddLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), laId, "test","ogd", false), "Legislative id already exists on cab");
            return Task.CompletedTask;
        }

        [Test]
        public async Task NewLegislativeId_AddLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var legislativeArea = new Mock<DocumentLegislativeArea>();
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId.ToString(),
                        StatusValue = Status.Draft,
                        DocumentLegislativeAreas = new()
                        {
                            legislativeArea.Object
                        }
                    }
                });

            // Act
            await _sut.AddLegislativeAreaAsync(new Mock<UserAccount>().Object, cabId, Guid.NewGuid(), "La to Add","ogd", false);

            // ClassicAssert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && d.DocumentLegislativeAreas.Contains(legislativeArea.Object)), null),
                Times.Once);            
        }
    }
}