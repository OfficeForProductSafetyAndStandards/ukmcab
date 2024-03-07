using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using System.Linq;
using UKMCAB.Data.Models.Users;

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
                await _sut.RemoveLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), Guid.NewGuid(), "Machinery"), "No document found");
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
                await _sut.RemoveLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), Guid.NewGuid(), "test"), "No legislative area found");
            return Task.CompletedTask;
        }

        [Test]
        public async Task LegislativeArea_RemoveLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var laToRemove = "La to Remove";
            var legislativeAreaId = Guid.NewGuid();
            var documentLegislativeArea = new DocumentLegislativeArea() { LegislativeAreaId = legislativeAreaId };
            var documentScopeOfAppointment = new DocumentScopeOfAppointment() { LegislativeAreaId = legislativeAreaId };
            var productSchedule = new FileUpload() { LegislativeArea = laToRemove };

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
                        Schedules = new () { productSchedule },
                    }
                });

            // Act
            await _sut.RemoveLegislativeAreaAsync(new Mock<UserAccount>().Object, cabId, legislativeAreaId, laToRemove);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && !d.DocumentLegislativeAreas.Contains(documentLegislativeArea) && !d.ScopeOfAppointments.Contains(documentScopeOfAppointment) && !d.Schedules.Contains(productSchedule) && !d.LegislativeAreas.Contains(laToRemove))), Times.Once);      
        }

        [Test]
        public Task DocumentNotFound_ArchiveLegislativeAreaAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.ArchiveLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), Guid.NewGuid()), "No document found");
            return Task.CompletedTask;
        }

        [Test]
        public Task LegislativeAreaNotFound_ArchiveLegislativeAreaAsync_ThrowsException()
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
                await _sut.ArchiveLegislativeAreaAsync(new Mock<UserAccount>().Object, Guid.NewGuid(), Guid.NewGuid()), "No legislative area found");
            return Task.CompletedTask;
        }

        [Test]
        public async Task LegislativeArea_ArchiveLegislativeAreaAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var legislativeAreaId = Guid.NewGuid();
            var documentLegislativeArea = new DocumentLegislativeArea() { LegislativeAreaId = legislativeAreaId };
            
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId.ToString(),
                        StatusValue = Status.Draft,
                        DocumentLegislativeAreas = new() { documentLegislativeArea } 
                    }
                });

            // Act
            await _sut.ArchiveLegislativeAreaAsync(new Mock<UserAccount>().Object, cabId, legislativeAreaId);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && d.DocumentLegislativeAreas.Contains(documentLegislativeArea) && d.DocumentLegislativeAreas.First().Archived == true)), Times.Once);
        }
    }
}