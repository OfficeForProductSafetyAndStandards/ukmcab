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
                new() {AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new() {AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new() {AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Archived }
            };
            var auditLog2 = new Audit { DateTime =  DateTime.Now, UserRole = "other" }; 
            var expectedOther = new List<Document>
            {
                new Document{AuditLog = new List<Audit>{auditLog2}, StatusValue = Status.Draft },
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                    !EvaluateDocumentPredicateWithoutRole(predicate))))
            .ReturnsAsync(expectedResults);
            
            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                    EvaluateDocumentPredicateWithoutRole(predicate))))
                .ReturnsAsync(expectedOther);
            
            // Act
            var ukasResults = await _sut.FindAllCABManagementQueueDocumentsForUserRole(role);

            // Assert
            CollectionAssert.AreEquivalent(expectedResults, ukasResults);
            CollectionAssert.AreNotEquivalent(expectedOther, ukasResults);
        }

        private bool EvaluateDocumentPredicateWithoutRole(Expression<Func<Document, bool>> predicate)
        {
            var body = predicate.Body as BinaryExpression;
            return body != null && !body.ToString().Contains("LastUserGroup");
        }
    }
}
