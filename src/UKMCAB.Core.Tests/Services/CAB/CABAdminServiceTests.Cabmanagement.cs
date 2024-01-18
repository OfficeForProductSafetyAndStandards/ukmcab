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
    public partial class CABAdminServiceTests
    {     
        private Mock<ICABRepository> _mockCABRepository = null!;
        private Mock<ICachedPublishedCABService> _mockCachedPublishedCAB = null!;
        private Mock<ICachedSearchService> _mockCachedSearchService = null!;
        private TelemetryClient _telemetryClient = null!;
        private Mock<IUserService> _mockUserService = null!;
        private ICABAdminService _sut = null!;
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
        
    }
}
