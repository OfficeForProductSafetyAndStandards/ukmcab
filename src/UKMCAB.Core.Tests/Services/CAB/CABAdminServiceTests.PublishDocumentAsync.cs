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
using UKMCAB.Data;
using UKMCAB.Core.Tests.Extensions;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task DocumentNotCreatedByOPSS_PublishDocumentAsync_LAsNotApprovedByOPSSAdminRemoved()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();
            
            var document = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object, document);

            // Assert
            Assert.That(9, Is.EqualTo(result.DocumentLegislativeAreas.Count));
            Assert.That(LAStatus.Published, Is.EqualTo(result.DocumentLegislativeAreas.First().Status));
            Assert.That(9, Is.EqualTo(result.ScopeOfAppointments.Count));
            Assert.That(8, Is.EqualTo(result.Schedules?.Count));
        }

        [Test]
        public async Task DocumentCreatedByOPSS_PublishDocumentAsync_AllLAsPublished()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var document = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                new Document
                {
                    CABId = Guid.NewGuid().ToString(),
                    CreatedByUserGroup = Roles.OPSS.Id,
                    StatusValue = Status.Draft,
                    SubStatus = SubStatus.None,
                    DocumentLegislativeAreas = legislativeAreas,
                    ScopeOfAppointments = scopeOfAppointments,
                });

            // Assert
            Assert.That(Enum.GetNames(typeof(LAStatus)).Length - 1, Is.EqualTo(result.DocumentLegislativeAreas.Count));
            Assert.That(result.DocumentLegislativeAreas.All(la => la.Status == LAStatus.Published), Is.True);
        }

        [Test]
        public async Task DocumentCreatedByOPSSAndSubmittedForOGDApproval_PublishDocumentAsync_AllLAsPublished()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var document = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules
            };

            var moqData = (new List<Document> { document }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                new Document
                {
                    CABId = Guid.NewGuid().ToString(),
                    CreatedByUserGroup = Roles.OPSS.Id,
                    StatusValue = Status.Draft,
                    SubStatus = SubStatus.PendingApprovalToPublish,
                    DocumentLegislativeAreas = new() 
                    {
                        new() {Status = LAStatus.ApprovedByOpssAdmin}, 
                        new() { Status = LAStatus.PendingApproval },
                        new() { Status = LAStatus.DeclinedByOpssAdmin },
                    },
                    ScopeOfAppointments = new()
                    {
                        new(){LegislativeAreaId = Guid.NewGuid()},
                    },
                });

            // Assert
            Assert.That(1, Is.EqualTo(result.DocumentLegislativeAreas.Count));
            Assert.That(result.DocumentLegislativeAreas.All(la => la.Status == LAStatus.Published), Is.True);
        }

        [Test]
        public async Task PublishDocumentAsync_ShouldReturnaDocumentWithALastUpdatedDateSameAsThePreviousPublishedVersion()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var latestDocument = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules,
                LastUpdatedDate = DateTime.MinValue
            };

            var lastUpdatedDate = new DateTime(2024, 1, 1);

            var publishDocument = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Published,
                SubStatus = SubStatus.None,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea> { },
                ScopeOfAppointments = new List<DocumentScopeOfAppointment> { },
                LastUpdatedDate = lastUpdatedDate
            };

            var moqData = (new List<Document> { latestDocument, publishDocument }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));

            _mockCABRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<DateTime?>()))
                       .Callback<Document, DateTime?>((doc, date) => doc.LastUpdatedDate = date ?? DateTime.Now)
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                latestDocument, null, null, DataConstants.PublishType.MinorPublish);

            // Assert
            Assert.That(latestDocument.LastUpdatedDate, Is.EqualTo(lastUpdatedDate));
            _mockCABRepository.Verify(repo => repo.UpdateAsync(latestDocument, lastUpdatedDate), Times.Once());
        }

        [Test]
        public async Task PublishDocumentAsync_ShouldReturnaDocumentWithCurrentDateIfNoPublishedOrArchivedDocumentExists()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var latestDocument = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules,
                LastUpdatedDate = DateTime.MinValue
            };


            var currentDateBefore = DateTime.Now;

            var moqData = (new List<Document> { latestDocument }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));
            _mockCABRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<DateTime?>()))
                       .Callback<Document, DateTime?>((doc, date) => doc.LastUpdatedDate = date ?? DateTime.Now)
                       .Returns(Task.CompletedTask);

            var lastUpdatedDate = DateTime.UtcNow;

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                latestDocument, null, null, DataConstants.PublishType.MinorPublish);

            var currentDateAfter = DateTime.Now;

            // Assert
            Assert.That(lastUpdatedDate.Date, Is.EqualTo(latestDocument.LastUpdatedDate.Date));
            _mockCABRepository.Verify(repo => repo.UpdateAsync(latestDocument, It.Is<DateTime>(d => d >= currentDateBefore && d <= currentDateAfter)), Times.Once);
        }

        [Test]
        public async Task PublishDocumentAsync_ShouldReturnaDocumentWithCurrentDateIfPublishType_IsMajorPublish()
        {
            // Arrange
            (var legislativeAreas, var scopeOfAppointments, var schedules) = GenerateTestData();

            var latestDocument = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Draft,
                DocumentLegislativeAreas = legislativeAreas,
                ScopeOfAppointments = scopeOfAppointments,
                Schedules = schedules,
                LastUpdatedDate = DateTime.MinValue
            };

            var lastUpdatedDate = new DateTime(2024, 1, 1);

            var publishDocument = new Document
            {
                CABId = Guid.NewGuid().ToString(),
                StatusValue = Status.Published,
                SubStatus = SubStatus.None,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea> { },
                ScopeOfAppointments = new List<DocumentScopeOfAppointment> { },
                LastUpdatedDate = lastUpdatedDate
            };

            var currentDateBefore = DateTime.Now;

            var moqData = (new List<Document> { latestDocument, publishDocument }).AsAsyncQueryable();
            _mockCABRepository.Setup(x => x.GetItemLinqQueryable()).Returns(moqData);
            _mockCABRepository.Setup(r => r.Query(It.IsAny<Expression<Func<Document, bool>>>()))
                .Returns<Expression<Func<Document, bool>>>(predicate => Task.FromResult(moqData.ToList()));
            _mockCABRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<DateTime?>()))
                       .Callback<Document, DateTime?>((doc, date) => doc.LastUpdatedDate = date ?? DateTime.Now)
                       .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PublishDocumentAsync(new Mock<UserAccount>().Object,
                latestDocument, null, null, DataConstants.PublishType.MajorPublish);

            var currentDateAfter = DateTime.Now;

            // Assert
            Assert.That(latestDocument.LastUpdatedDate >= currentDateBefore && latestDocument.LastUpdatedDate <= currentDateAfter, Is.True);
            _mockCABRepository.Verify(repo => repo.UpdateAsync(latestDocument, It.Is<DateTime>(d => d >= currentDateBefore && d <= currentDateAfter)), Times.Once);
        }

        private (List<DocumentLegislativeArea>, List<DocumentScopeOfAppointment>, List<FileUpload>) GenerateTestData()
        {
            var legislativeAreas = new List<DocumentLegislativeArea>();
            var scopeOfAppointments = new List<DocumentScopeOfAppointment>();
            var schedules = new List<FileUpload>();

            foreach (var status in Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s => s != LAStatus.Published))
            {
                var legislativeAreaId = Guid.NewGuid();
                legislativeAreas.Add(new DocumentLegislativeArea
                {
                    LegislativeAreaId = legislativeAreaId,
                    LegislativeAreaName = legislativeAreaId.ToString(),
                    Status = status,
                });
                scopeOfAppointments.Add(new DocumentScopeOfAppointment
                {
                    LegislativeAreaId = legislativeAreaId,
                });
                schedules.Add(new FileUpload
                {
                    Id = Guid.NewGuid(),
                    LegislativeArea = legislativeAreaId.ToString(),
                });
            }

            return (legislativeAreas, scopeOfAppointments, schedules);
        }
    }
}