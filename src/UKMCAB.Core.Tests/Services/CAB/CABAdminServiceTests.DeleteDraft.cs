using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using System.Linq;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {     
        [Test]
        public async Task DeleteDraftDocumentAsync_ShouldDoNothingIfDraftCabNotFound()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.DeleteAsync(It.IsAny<Document>()), Times.Never);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }

        [Test]
        public Task DeleteDraftDocumentAsync_ShouldErrorIfDeleteReasonIsBlankAndCabHasPublishedVersion()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Published },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            // Act
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), null));

            // Assert
            Assert.AreEqual("The delete reason must be specified when an earlier document version exists.", exception.Message);

            _mockCABRepository.Verify(x => x.DeleteAsync(It.IsAny<Document>()), Times.Never);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
            return Task.CompletedTask;
        }

        [Test]
        public Task DeleteDraftDocumentAsync_ShouldErrorIfRepositoryDeleteReturnsFalse()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Published },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.Is<Document>(x => x.id == "2" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(false);

            // Act
            var cabId = Guid.NewGuid();
            Exception exception = Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteDraftDocumentAsync(new UserAccount(), cabId, _faker.Random.Word()));

            // Assert
            Assert.AreEqual($"Failed to delete draft version, CAB Id: {cabId}", exception.Message);

            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
            _mockCachedSearchService.Verify(x => x.RemoveFromIndexAsync(It.IsAny<string>()), Times.Never);
            _mockCachedSearchService.Verify(x => x.ClearAsync(), Times.Never);
            _mockCachedSearchService.Verify(x => x.ClearAsync(It.IsAny<string>()), Times.Never);
            _mockCachedPublishedCAB.Verify(x => x.ClearAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            return Task.CompletedTask;
        }

        [Test]
        public async Task DeleteDraftDocumentAsync_ShouldUpdateSearchIndexAndCachesIfDeleteSuccessful()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", CABId = cabId.ToString(), StatusValue = Status.Draft, URLSlug = "urlSlug" },
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), cabId, _faker.Random.Word());

            // Assert
            _mockCachedSearchService.Verify(x => x.RemoveFromIndexAsync("1"), Times.Once);
            _mockCachedSearchService.Verify(x => x.ClearAsync(), Times.Once);
            _mockCachedSearchService.Verify(x => x.ClearAsync(cabId.ToString()), Times.Once);
            _mockCachedPublishedCAB.Verify(x => x.ClearAsync(cabId.ToString(), "urlSlug"), Times.Once);
        }

        [Test]
        public async Task DeleteDraftDocumentAsync_ShouldAddEntryToAuditLogIfCabHasPublishedVersion()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Published, AuditLog = new List<Audit> { } },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.Is<Document>(x => x.id == "2" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Published && 
                x.AuditLog.Count == 1 && x.AuditLog.First().Action == AuditCABActions.DraftDeleted)), Times.Once);
        }

        [Test]
        public async Task DeleteDraftDocumentAsync_ShouldNotAddEntryToAuditLogIfCabHasNoPublishedVersion()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }

        [Test]
        public async Task DeleteDraftDocumentAsync_ShouldResetSubstatusOnPreviousVersionIfCreatedViaRequestToUnarchive()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Archived, SubStatus = SubStatus.PendingApprovalToUnarchive, AuditLog = new List<Audit> { } },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.DeleteAsync(It.Is<Document>(x => x.id == "2" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), cabId, _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Archived &&
                x.SubStatus == SubStatus.None && x.SubStatusName == "None")), Times.Once);

            _mockCachedSearchService.Verify(x => x.ReIndexAsync(It.Is<CABIndexItem>(x => x.id == "1" && x.SubStatus == ((int)SubStatus.None).ToString())), Times.Once);
        }
    }
}
