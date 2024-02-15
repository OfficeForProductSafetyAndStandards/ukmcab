using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public Task DocumentNotFound_RemoveLegislativeAreaAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.RemoveLegislativeAreaAsync(Guid.NewGuid(), Guid.NewGuid(), "Machinery"), "No document found");
            return Task.CompletedTask;
        }

        [Test]
        public Task LegislativeAreaNotFound_RemoveLegislativeAreaAsync_ThrowsException()
        {
            // Arrange         
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        id = Guid.NewGuid().ToString()
                    }
                });

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.RemoveLegislativeAreaAsync(Guid.NewGuid(), Guid.NewGuid(), "test"), "No legislative area found");
            return Task.CompletedTask;
        }

        [Test]
        public async Task LegislativeArea_RemoveLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var documentLegislativeArea = new DocumentLegislativeArea() { LegislativeAreaId = legislativeAreaId };
            var documentScopeOfAppointment = new DocumentScopeOfAppointment() { LegislativeAreaId = legislativeAreaId };  
            var laToRemove = "La to Remove";
           
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId.ToString(),
                        StatusValue = Status.Draft,
                        DocumentLegislativeAreas = new() { documentLegislativeArea } ,
                        ScopeOfAppointments = new() { documentScopeOfAppointment },
                        LegislativeAreas = new() { laToRemove },
                    }
                });

            // Act
            await _sut.RemoveLegislativeAreaAsync(cabId, legislativeAreaId, laToRemove);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && !d.DocumentLegislativeAreas.Contains(documentLegislativeArea) && !d.ScopeOfAppointments.Contains(documentScopeOfAppointment) && !d.LegislativeAreas.Contains(laToRemove))), Times.Once);
           

            _mockCABRepository.VerifyNoOtherCalls();

        }
    }
}