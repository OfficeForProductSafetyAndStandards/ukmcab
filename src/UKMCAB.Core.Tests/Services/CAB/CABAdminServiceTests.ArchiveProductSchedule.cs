using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using System.Linq;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public Task DocumentNotFound_ArchiveScheduleAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _sut.ArchiveSchedulesAsync(Guid.NewGuid(), new List<Guid> { Guid.NewGuid() }), "No document found");
            return Task.CompletedTask;
        }

        [Test]
        public async Task ArchiveScheduleAsync_CabNotUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            var scheduleId = Guid.NewGuid();
            var scheduleIds = new List<Guid> { scheduleId };
            var productSchedule = new FileUpload() { Id = Guid.NewGuid(), Archived = false };

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId.ToString(),
                        StatusValue = Status.Draft,
                        Schedules = new () { productSchedule },
                    }
                });

            // Act
            await _sut.ArchiveSchedulesAsync(cabId, scheduleIds);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && d.Schedules.Contains(productSchedule) && d.Schedules.First().Archived == false)), Times.Never);

            _mockCABRepository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ArchiveScheduleAsync_CabUpdated()
        {
            // Arrange
            var cabId = Guid.NewGuid();            
            var scheduleId = Guid.NewGuid();
            var scheduleIds = new List<Guid> { scheduleId };            
            var productSchedule = new FileUpload() { Id = scheduleId };           
           
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId.ToString(),
                        StatusValue = Status.Draft,
                        Schedules = new () { productSchedule },
                    }
                });

            // Act
            await _sut.ArchiveSchedulesAsync(cabId, scheduleIds);
            
            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(
                r => r.UpdateAsync(It.Is<Document>(d =>
                    d.CABId == cabId.ToString() && d.Schedules.Contains(productSchedule) && d.Schedules.First().Archived == true)), Times.Once);

            _mockCABRepository.VerifyNoOtherCalls();
        }
    }
}