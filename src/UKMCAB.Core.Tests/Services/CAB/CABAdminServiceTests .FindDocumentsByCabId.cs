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
        public async Task FindAllDocumentsByCABIdAsync_ReturnsEmptyList()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act 
            var result = await _sut.FindAllDocumentsByCABIdAsync(_faker.Random.Word());
            
            // Assert
            Assert.False(result.Any());
        }        
        
        
        [Test]
        public async Task FindAllDocumentsByCABIdAsync_ReturnsList()
        {
            var auditLog1 = new Audit { DateTime = DateTime.Now.AddDays(1) }; // audit log
            var auditLog2 = new Audit { DateTime = DateTime.Now.AddDays(2) }; // audit log
            var auditLog3 = new Audit { DateTime = DateTime.Now.AddDays(3)}; // audit log

            var expectedResults = new List<Document>
            {
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(),  StatusValue = Status.Draft, AuditLog = new List<Audit>{auditLog1} },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(),  StatusValue = Status.Draft, AuditLog = new List<Audit>{auditLog2} },
                new() {id = Guid.NewGuid().ToString(), CABId = Guid.NewGuid().ToString(),  StatusValue = Status.Archived, AuditLog = new List<Audit>{auditLog3}}
            };

            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
               .ReturnsAsync(expectedResults);

            // Act 
            var result = await _sut.FindAllDocumentsByCABIdAsync(_faker.Random.Word());

            // Assert
            Assert.AreEqual(result[0].CABId, expectedResults[2].CABId);
            Assert.AreEqual(result[1].CABId, expectedResults[1].CABId);
            Assert.AreEqual(result[2].CABId, expectedResults[0].CABId);
        }      
    }
}