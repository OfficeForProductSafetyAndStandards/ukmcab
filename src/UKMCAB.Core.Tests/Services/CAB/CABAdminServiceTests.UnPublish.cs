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
        public Task DocumentNotFound_UnpublishDocumentAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act and Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.UnPublishDocumentAsync(new Mock<UserAccount>().Object, _faker.Random.Word(), null));
            return Task.CompletedTask;
        }

        [Test]
        public Task IncorrectStatus_UnpublishDocumentAsync_ThrowsException()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        StatusValue = _faker.Random.Enum(Status.Published)
                    }
                });

            // Act and Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.UnPublishDocumentAsync(new Mock<UserAccount>().Object, _faker.Random.Word(), null));
            return Task.CompletedTask;
        }

        [Test]
        public Task DraftFailsToDelete_UnpublishDocumentAsync_ThrowsException()
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
                        StatusValue = Status.Draft
                    }
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.IsAny<Document>())).ReturnsAsync(false);

            // Act and Assert
            Assert.ThrowsAsync<Exception>(async () =>
                await _sut.UnPublishDocumentAsync(new Mock<UserAccount>().Object, _faker.Random.Word(), null));
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(r => r.DeleteAsync(It.IsAny<Document>()), Times.Once());
            _mockCABRepository.VerifyNoOtherCalls();
            return Task.CompletedTask;
        }

        [Test]
        public async Task DocumentFound_UnpublishDocumentAsync_CabUpdated()
        {
            // Arrange
            const string cabId = "cabId";
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId,
                        StatusValue = Status.Published
                    }
                });
            _mockCABRepository.Setup(x => x.DeleteAsync(It.IsAny<Document>())).ReturnsAsync(false);
            
            // Act
            await _sut.UnPublishDocumentAsync(new Mock<UserAccount>().Object, cabId, null);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(r => r.UpdateAsync(It.Is<Document>(d => d.CABId == cabId && d.AuditLog.First().Action == AuditCABActions.UnpublishApprovalRequest)), Times.Once);
            _mockCABRepository.Verify(r => r.GetItemLinqQueryable(), Times.Exactly(5));
            _mockCABRepository.VerifyNoOtherCalls();
        }
        
        [Test]
        public async Task DocumentFound_UnpublishDocumentAsync_CabUpdatesStatuses()
        {
            // Arrange
            const string cabId = "cabId";
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new()
                    {
                        CABId = cabId,
                        StatusValue = Status.Published
                    }
                });
            _mockCABRepository.Setup(x => x.DeleteAsync(It.IsAny<Document>())).ReturnsAsync(false);
            
            // Act
            await _sut.UnPublishDocumentAsync(new Mock<UserAccount>().Object, cabId, null);

            // Assert
            _mockCABRepository.Verify(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()), Times.Once);
            _mockCABRepository.Verify(r => r.UpdateAsync(It.Is<Document>(d => d.StatusValue == Status.Historical && d.SubStatus == SubStatus.None)), Times.Once);
            _mockCABRepository.Verify(r => r.GetItemLinqQueryable(), Times.Exactly(5));
            _mockCABRepository.VerifyNoOtherCalls();
        }
    }
}