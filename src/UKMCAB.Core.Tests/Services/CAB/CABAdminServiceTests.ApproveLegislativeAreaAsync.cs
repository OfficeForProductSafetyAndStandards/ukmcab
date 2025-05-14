using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using System.Linq;
using UKMCAB.Data.Models.Users;
using UKMCAB.Core.Tests.Extensions;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task ApproveLegislativeAreaAsync_AddNewAuditLogToDocument()
        {
            // Arrange
            var userAccount = new UserAccount
            {
                Id = "Test id",
                FirstName = "Test",
                Surname = "User",
                Role = "Test role"
            };
            var legislativeAreaId = Guid.NewGuid();
            var document = new Document
            {
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        LegislativeAreaId = legislativeAreaId
                    }
                }
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            await _sut.ApproveLegislativeAreaAsync(userAccount, Guid.NewGuid(), legislativeAreaId, LAStatus.Approved);

            // Assert
            var auditLog = document.AuditLog.Single();

            Assert.That(1, Is.EqualTo(document.AuditLog.Count));
            Assert.That(userAccount.Id, Is.EqualTo(auditLog.UserId));
            Assert.That($"{userAccount.FirstName} {userAccount.Surname}", Is.EqualTo(auditLog.UserName));
            Assert.That(userAccount.Role, Is.EqualTo(auditLog.UserRole));
            Assert.That(AuditCABActions.ApproveLegislativeArea, Is.EqualTo(auditLog.Action));
            Assert.That(null, Is.EqualTo(auditLog.Comment));
            Assert.That(null, Is.EqualTo(auditLog.PublicComment));
            var now = DateTime.UtcNow;
            Assert.That(now.Year, Is.EqualTo(auditLog.DateTime.Year));
            Assert.That(now.Month, Is.EqualTo(auditLog.DateTime.Month));
            Assert.That(now.Day, Is.EqualTo(auditLog.DateTime.Day));
            Assert.That(now.Hour, Is.EqualTo(auditLog.DateTime.Hour));
            Assert.That(now.Minute, Is.EqualTo(auditLog.DateTime.Minute));
        }

        [Test]
        public async Task DeclineLegislativeAreaAsync_AddNewAuditLogToDocument()
        {
            // Arrange
            var userAccount = new UserAccount
            {
                Id = "Test id",
                FirstName = "Test",
                Surname = "User",
                Role = "Test role"
            };
            var legislativeAreaId = Guid.NewGuid();
            var document = new Document
            {
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        LegislativeAreaId = legislativeAreaId,
                        LegislativeAreaName = "Test name"
                    }
                }
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            await _sut.DeclineLegislativeAreaAsync(userAccount, Guid.NewGuid(), legislativeAreaId, "Test reason", LAStatus.Approved);

            // Assert
            var auditLog = document.AuditLog.Single();

            Assert.That(1, Is.EqualTo(document.AuditLog.Count));
            Assert.That(userAccount.Id, Is.EqualTo(auditLog.UserId));
            Assert.That($"{userAccount.FirstName} {userAccount.Surname}", Is.EqualTo(auditLog.UserName));
            Assert.That(userAccount.Role, Is.EqualTo(auditLog.UserRole));
            Assert.That(AuditCABActions.DeclineLegislativeArea, Is.EqualTo(auditLog.Action));
            Assert.That("Legislative area Test name declined: </br>Test reason", Is.EqualTo(auditLog.Comment));
            Assert.That(null, Is.EqualTo(auditLog.PublicComment));

            var now = DateTime.UtcNow;
            var expectedDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            Assert.That(expectedDate, Is.EqualTo(new DateTime(auditLog.DateTime.Year, auditLog.DateTime.Month, auditLog.DateTime.Day, auditLog.DateTime.Hour, auditLog.DateTime.Minute, auditLog.DateTime.Second)));
        }
    }
}