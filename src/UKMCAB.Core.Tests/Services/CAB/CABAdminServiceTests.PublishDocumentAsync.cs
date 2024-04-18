using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using System.Linq;
using UKMCAB.Core.Security;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        private List<DocumentLegislativeArea> DocumentLegislativeAreas() => Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>()
            .Where(s => s != LAStatus.Published)
            .Select(s => new DocumentLegislativeArea
            {
                Status = s,
            }).ToList();

        [Test]
        public async Task DocumentFoundAndNotCreatedByOPSS_PublishDocumentAsync_LAsNotApprovedByOPSSAdminRemoved()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object, 
                new Document
                { 
                    StatusValue = Status.Draft, 
                    DocumentLegislativeAreas = DocumentLegislativeAreas()
                });

            // Assert
            Assert.AreEqual(1, result.DocumentLegislativeAreas.Count);
            Assert.AreEqual(LAStatus.Published, result.DocumentLegislativeAreas.First().Status);
        }

        [Test]
        public async Task DocumentFoundAndCreatedByOPSS_PublishDocumentAsync_AllLAsPublished()
        {
            // Arrange
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                new Document
                {
                    CreatedByUserGroup = Roles.OPSS.Id,
                    StatusValue = Status.Draft,
                    DocumentLegislativeAreas = DocumentLegislativeAreas()
                });

            // Assert
            Assert.AreEqual(13, result.DocumentLegislativeAreas.Count);
            Assert.True(result.DocumentLegislativeAreas.All(la => la.Status == LAStatus.Published));
        }
    }
}