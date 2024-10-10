using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using NUnit.Framework.Legacy;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task SingleDraftDocAsync_DocumentEmptyList_ReturnsFalse()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>());

            // Act 
            var result = await _sut.IsSingleDraftDocAsync(Guid.NewGuid());
            
            // ClassicAssert
            ClassicAssert.False(result);
        }

        [Test]
        public async Task SingleDraftDocAsync_DocumentList_ReturnsTrue()
        {
            var cabId = Guid.NewGuid();

            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new ()
                    {
                        id = cabId.ToString(),
                        StatusValue = Status.Draft
                    },
                    new ()
                    {
                       id = Guid.NewGuid().ToString(),
                        StatusValue = Status.Published
                    },
                    new ()
                    {
                       id = Guid.NewGuid().ToString(),
                       StatusValue = Status.Historical
                    }
                });

            // Act 
            var result = await _sut.IsSingleDraftDocAsync(cabId);

            // ClassicAssert
            ClassicAssert.True(true);
        }

        [Test]
        public async Task SingleDraftDocAsync_DocumentList_ReturnsFalse()
        {
            var cabId = Guid.NewGuid();

            // Arrange
            _mockCABRepository.Setup(x => x.Query<Document>(It.IsAny<Expression<Func<Document, bool>>>()))
                .ReturnsAsync(new List<Document>
                {
                    new ()
                    {
                        id = cabId.ToString(),
                        StatusValue = Status.Draft
                    },
                    new ()
                    {
                       id = cabId.ToString(),
                        StatusValue = Status.Published
                    },
                    new ()
                    {
                       id = cabId.ToString(),
                       StatusValue = Status.Historical
                    }
                });

            // Act 
            var result = await _sut.IsSingleDraftDocAsync(cabId);

            // ClassicAssert
            ClassicAssert.False(result);
        }       
    }
}
