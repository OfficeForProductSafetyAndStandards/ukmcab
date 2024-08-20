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

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document> 
            {
                document
            });

            // Act
            await _sut.ApproveLegislativeAreaAsync(userAccount, Guid.NewGuid(), legislativeAreaId, LAStatus.Approved);

            // Assert
            var auditLog = document.AuditLog.Single();

            Assert.AreEqual(1, document.AuditLog.Count);
            Assert.AreEqual(userAccount.Id, auditLog.UserId);
            Assert.AreEqual($"{userAccount.FirstName} {userAccount.Surname}", auditLog.UserName);
            Assert.AreEqual(userAccount.Role, auditLog.UserRole);
            Assert.AreEqual(AuditCABActions.ApproveLegislativeArea, auditLog.Action);
            Assert.AreEqual(null, auditLog.Comment);
            Assert.AreEqual(null, auditLog.PublicComment);
            var now = DateTime.UtcNow;
            Assert.AreEqual(now.Year, auditLog.DateTime.Year);
            Assert.AreEqual(now.Month, auditLog.DateTime.Month);
            Assert.AreEqual(now.Day, auditLog.DateTime.Day);
            Assert.AreEqual(now.Hour, auditLog.DateTime.Hour);
            Assert.AreEqual(now.Minute, auditLog.DateTime.Minute);
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

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>
            {
                document
            });

            // Act
            await _sut.DeclineLegislativeAreaAsync(userAccount, Guid.NewGuid(), legislativeAreaId, "Test reason", LAStatus.Approved);

            // Assert
            var auditLog = document.AuditLog.Single();

            Assert.AreEqual(1, document.AuditLog.Count);
            Assert.AreEqual(userAccount.Id, auditLog.UserId);
            Assert.AreEqual($"{userAccount.FirstName} {userAccount.Surname}", auditLog.UserName);
            Assert.AreEqual(userAccount.Role, auditLog.UserRole);
            Assert.AreEqual(AuditCABActions.DeclineLegislativeArea, auditLog.Action);
            Assert.AreEqual("Legislative area Test name declined: </br>Test reason", auditLog.Comment);
            Assert.AreEqual(null, auditLog.PublicComment);

            var now = DateTime.UtcNow;
            var expectedDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            Assert.AreEqual(expectedDate, new DateTime(auditLog.DateTime.Year, auditLog.DateTime.Month, auditLog.DateTime.Day, auditLog.DateTime.Hour, auditLog.DateTime.Minute, auditLog.DateTime.Second));
        }
    }
}