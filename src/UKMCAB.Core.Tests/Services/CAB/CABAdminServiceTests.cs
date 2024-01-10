using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using Microsoft.ApplicationInsights;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Core.Mappers;
using System.Linq;
using Bogus;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public class CABAdminServiceTests
    {     
        private Mock<ICABRepository> _mockCABRepository;
        private Mock<ICachedPublishedCABService> _mockCachedPublishedCAB;
        private Mock<ICachedSearchService> _mockCachedSearchService;
        private TelemetryClient _telemetryClient;
        private Mock<IUserService> _mockUserService;
        private ICABAdminService _sut;
        private readonly Faker _faker = new();

        [SetUp] 
        public void Setup() {
            _mockCABRepository = new Mock<ICABRepository>();
            _mockCachedPublishedCAB = new Mock<ICachedPublishedCABService>();
            _mockCachedSearchService = new Mock<ICachedSearchService>();
            _telemetryClient = new TelemetryClient();
            _mockUserService = new Mock<IUserService>();

            _sut = new CABAdminService(_mockCABRepository.Object, _mockCachedSearchService.Object,_mockCachedPublishedCAB.Object, _telemetryClient, _mockUserService.Object);
        }

        [Theory]
        [TestCase("ukas")]
        public async Task FindAllCABManagementQueueDocumentsForUserRole_UKAS_ShouldReturnFilteredResults(string role)
        {
            // Arrange
            var auditLog1 = new Audit { DateTime =  DateTime.Now, UserRole = role }; // Role audit log
            var expectedResults = new List<Document>
            {
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Archived }
            };

            var auditLog2 = new Audit { DateTime =  DateTime.Now, UserRole = "other" }; 

            var expectedOther = new List<Document>
            {
                new Document{
                    id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(), AuditLog = new List<Audit>{auditLog2}, StatusValue = Status.Draft 
                }
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                    !EvaluateDocumentPredicateWithoutRole(predicate))))
                .ReturnsAsync(expectedResults);
            
            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                    EvaluateDocumentPredicateWithoutRole(predicate))))
                .ReturnsAsync(expectedOther);
            
            // Act
            var ukasResults = await _sut.FindAllCABManagementQueueDocumentsForUserRole(role);

            var expectedResultsCABModel = expectedResults.Select(d => d.MapToCabModel()).ToList();
            var expectedOtherCABModel = expectedOther.Select(d => d.MapToCabModel()).ToList();

            // Assert
            Assert.AreEqual(ukasResults[0].CABId, expectedResultsCABModel[0].CABId);
            Assert.AreEqual(ukasResults[1].CABId, expectedResultsCABModel[1].CABId);
            Assert.AreEqual(ukasResults[2].CABId, expectedResultsCABModel[2].CABId);

            Assert.AreNotEqual(ukasResults[0].CABId, expectedOtherCABModel[0].CABId);
       
        }

        private bool EvaluateDocumentPredicateWithoutRole(Expression<Func<Document, bool>> predicate)
        {
            var body = predicate.Body as BinaryExpression;
            return body != null && !body.ToString().Contains("CreatedByUserGroup");
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldDoNothingIfDraftCabNotFound()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.Delete(It.IsAny<Document>()), Times.Never);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldErrorIfDeleteReasonIsBlankAndCabHasPublishedVersion()
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

            _mockCABRepository.Verify(x => x.Delete(It.IsAny<Document>()), Times.Never);
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldErrorIfRepositoryDeleteReturnsFalse()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Published },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.Delete(It.Is<Document>(x => x.id == "2" && x.StatusValue == Status.Draft)))
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
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldUpdateSearchIndexAndCachesIfDeleteSuccessful()
        {
            // Arrange
            var cabId = Guid.NewGuid();
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", CABId = cabId.ToString(), StatusValue = Status.Draft, URLSlug = "urlSlug" },
                });

            _mockCABRepository.Setup(x => x.Delete(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), cabId, _faker.Random.Word());

            // Assert
            _mockCachedSearchService.Verify(x => x.RemoveFromIndexAsync("1"), Times.Once);
            _mockCachedSearchService.Verify(x => x.ClearAsync(), Times.Once);
            _mockCachedSearchService.Verify(x => x.ClearAsync(cabId.ToString()), Times.Once);
            _mockCachedPublishedCAB.Verify(x => x.ClearAsync(cabId.ToString(), "urlSlug"), Times.Once);
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldAddEntryToAuditLogIfCabHasPublishedVersion()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Published, AuditLog = new List<Audit> { } },
                    new Document { id = "2", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.Delete(It.Is<Document>(x => x.id == "2" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Published && 
                x.AuditLog.Count == 1 && x.AuditLog.First().Action == AuditCABActions.DraftDeleted)), Times.Once);
        }

        [TestCase]
        public async Task DeleteDraftDocumentAsync_ShouldNotAddEntryToAuditLogIfCabHasNoPublishedVersion()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>() {
                    new Document { id = "1", StatusValue = Status.Draft },
                });

            _mockCABRepository.Setup(x => x.Delete(It.Is<Document>(x => x.id == "1" && x.StatusValue == Status.Draft)))
                .ReturnsAsync(true);

            // Act
            await _sut.DeleteDraftDocumentAsync(new UserAccount(), Guid.NewGuid(), _faker.Random.Word());

            // Assert
            _mockCABRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>()), Times.Never);
        }
    }
}
