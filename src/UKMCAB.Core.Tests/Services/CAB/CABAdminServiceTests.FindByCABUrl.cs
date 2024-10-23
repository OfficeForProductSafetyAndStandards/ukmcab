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
        public async Task DocumentNotFound_FindAllDocumentsByCABURLAsync_ReturnsEmptyList()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act 
            var result = await _sut.FindAllDocumentsByCABURLAsync(_faker.Random.Word());
            
            // Assert
            Assert.That(result.Count != 0, Is.False);
        }
        
        [Test]
        public async Task EmptyStatuses_FindAllDocumentsByCABURLAsync_ReturnsFullList()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new (),
                    new (),
                    new ()
                });

            // Act 
            var result = await _sut.FindAllDocumentsByCABURLAsync(_faker.Random.Word());
            
            // Assert
            Assert.That(3, Is.EqualTo(result.Count));
        }
        
        [Test]
        public async Task StatusesProvided_FindAllDocumentsByCABURLAsync_ReturnsFilteredList()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.Is<Expression<Func<Document, bool>>>(e => EvaluateDocumentPredicateStatusesToRetrieve(e))))
                .ReturnsAsync(new List<Document>
                {
                    new ()
                    {
                        StatusValue = Status.Historical
                    },
                    new ()
                    {
                        StatusValue = Status.Historical
                    },
                    new ()
                    {
                        StatusValue = Status.Historical
                    }
                });

            // Act 
            var result = await _sut.FindAllDocumentsByCABURLAsync(_faker.Random.Word(), new Status[]
            {
                Status.Historical
            });
            
            // Assert
            Assert.That(3, Is.EqualTo(result.Count));
            foreach (var doc in result)
            {
                Assert.That(Status.Historical, Is.EqualTo(doc.StatusValue));
            }
        }
        
        
        private bool EvaluateDocumentPredicateStatusesToRetrieve(Expression<Func<Document, bool>> predicate)
        {
            var body = predicate.Body as BinaryExpression;
            return body != null && body.Right.ToString().Contains("statusesToRetrieve");
        }
    }
}