using NUnit.Framework;
using Moq;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using Microsoft.ApplicationInsights;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Search.Services;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Core.Security;
using UKMCAB.Core.Tests.FakeRepositories;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public class CABAdminServiceTests
    {     
        private Mock<ICABRepository> _mockCABRepository;
        //private ICABRepository _cabRepository;
        private Mock<ICachedPublishedCABService> _mockCachedPublishedCAB;
        private Mock<ICachedSearchService> _mockCachedSearchService;
        private TelemetryClient _telemetryClient;
        private Mock<IUserService> _mockUserService;
        private ICABAdminService _sut;

        [SetUp] 
        public void Setup() {
            _mockCABRepository = new Mock<ICABRepository>();
            //_cabRepository = new FakeCABRepository();
            _mockCachedPublishedCAB = new Mock<ICachedPublishedCABService>();
            _mockCachedSearchService = new Mock<ICachedSearchService>();
            _telemetryClient = new TelemetryClient();
            _mockUserService = new Mock<IUserService>();

            _sut = new CABAdminService(_mockCABRepository.Object, _mockCachedSearchService.Object,_mockCachedPublishedCAB.Object, _telemetryClient, _mockUserService.Object);
        }

        [Test]  
        public async Task FindAllCABManagementQueueDocumentsForUserRole_UKAS_ShouldReturnFilteredResults()
        {
            // Arrange
            var auditLog1 = new Audit { DateTime =  DateTime.Now, UserRole = Roles.UKAS.Id }; // UKAS audit log

            var expectedUKASResults = new List<Document>
            {
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Archived }
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                EvaluateDocumentPredicateForUKASRole(predicate, Roles.UKAS.Id))))
            .ReturnsAsync(expectedUKASResults);


            // Act
            var ukasResults = await _sut.FindAllCABManagementQueueDocumentsForUserRole(Roles.UKAS.Id);


            // Assert
            //Assert.AreEqual(expectedResult, ukasResults);
            CollectionAssert.AreEquivalent(expectedUKASResults, ukasResults);
        }

        [Test]  
        public async Task FindAllCABManagementQueueDocumentsForUserRole_OPSS_ShouldReturnAllResults()
        {
            // Arrange
            var auditLog1 = new Audit { DateTime =  DateTime.Now, UserRole = Roles.UKAS.Id }; // UKAS audit log
            var auditLog2 = new Audit { DateTime =  DateTime.Now, UserRole = Roles.OPSS.Id }; // OPSS audit log

            var expectedOPSSResults = new List<Document>
            {
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog2}, StatusValue = Status.Draft },
                new Document{AuditLog = new List<Audit>{auditLog1}, StatusValue = Status.Archived }
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.Is<Expression<Func<Document, bool>>>(predicate =>
                EvaluateDocumentPredicateForOPSSRole(predicate, Roles.OPSS.Id))))
            .ReturnsAsync(expectedOPSSResults);


            // Act
            var opssResults = await _sut.FindAllCABManagementQueueDocumentsForUserRole(Roles.OPSS.Id);


            // Assert
            CollectionAssert.AreEquivalent(expectedOPSSResults, opssResults);
        }



        private bool EvaluateDocumentPredicateForUKASRole(Expression<Func<Document, bool>> predicate, string userRole)
        {
            var body = predicate.Body as BinaryExpression;

            if (userRole == Roles.UKAS.Id && body != null)
            {
                return true;
            }

            return false;
        }

        private bool EvaluateDocumentPredicateForOPSSRole(Expression<Func<Document, bool>> predicate, string userRole)
        {
            var body = predicate.Body as BinaryExpression;

            if (userRole == Roles.OPSS.Id && body != null)
            {
                return true;
            }

            return false;
        }
    }
}
