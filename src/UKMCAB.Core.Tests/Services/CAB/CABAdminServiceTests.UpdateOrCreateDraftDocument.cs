using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Core.Tests.Services.CAB
{
    [TestFixture]
    public partial class CABAdminServiceTests
    {
        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_ShouldReturnDraftDocumentWithSubstatus_PendingApprovalToPublish_WhenSubmittedForApproval()
        {
            // Arrange
            var userAccount = new UserAccount();
            var submitForApproval = true;
            var draftDocument = new Document 
            { 
                CABId = "2efe970d-cb83-4f1e-9ced-5489de4af8ca",
                URLSlug = "Test-Kab-17-04-24",
                StatusValue = Status.Draft, 
                SubStatus = SubStatus.None,
                DocumentLegislativeAreas =  new List<DocumentLegislativeArea>
                {
                    new DocumentLegislativeArea { Status = LAStatus.Draft},
                    new DocumentLegislativeArea { Status = LAStatus.Draft},
                    new DocumentLegislativeArea { Status = LAStatus.Draft},
                },
                
            };

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument, submitForApproval);
            
            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.PendingApprovalToPublish);
            Assert.AreEqual(result.AuditLog.First().Action, AuditCABActions.SubmittedForApproval);
            Assert.IsTrue(result.DocumentLegislativeAreas.All(x => x.Status == LAStatus.PendingApproval));
            _mockCABRepository.Verify(r => r.UpdateAsync(draftDocument, null), Times.Once);
        }

        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_ShouldReturnDraftDocumentWithStatus_Draft_WhenDocumentStatusIsPublished()
        {
            // Arrange
            var userAccount = new UserAccount {Role = "opss" };
            var draftDocument = new Document
            {
                CABId = "2efe970d-cb83-4f1e-9ced-5489de4af8ca",
                StatusValue = Status.Published,
            };

            var createdDocument = new Document();
            Document intermediateDocument = null;
            _mockCABRepository.Setup(r => r.CreateAsync(It.IsAny<Document>(), It.IsAny<DateTime>())).Callback<Document, DateTime>((doc, _) => intermediateDocument = doc).ReturnsAsync(createdDocument);            

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument);

            // Assert
            Assert.AreEqual(intermediateDocument.StatusValue, Status.Draft);
            Assert.AreEqual(intermediateDocument.AuditLog.First().Action, AuditCABActions.Created);
            Assert.That(intermediateDocument.CreatedByUserGroup, Is.EqualTo("opss"));
            _mockCABRepository.Verify(r => r.CreateAsync(draftDocument, It.IsAny<DateTime>()), Times.Once);
        }

        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_ShouldReturnDraftDocumentWithSubstatus_None_WhenAllLAsAreCombinationOfPublishedOrAndDeclined()
        {
            // Arrange
            var userAccount = new UserAccount();
            var draftDocument = new Document
            {
                CABId = "2efe970d-cb83-4f1e-9ced-5489de4af8ca",
                URLSlug = "Test-Kab-17-04-24",
                StatusValue = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new DocumentLegislativeArea { Status = LAStatus.Published},
                    new DocumentLegislativeArea { Status = LAStatus.Declined},
                    new DocumentLegislativeArea { Status = LAStatus.DeclinedByOpssAdmin},
                    new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS},
                    new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS},
                    new DocumentLegislativeArea { Status = LAStatus.DeclinedToRemoveByOPSS},
                    new DocumentLegislativeArea { Status = LAStatus.DeclinedToUnarchiveByOPSS},
                },
            };

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument);

            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.None);
            _mockCABRepository.Verify(r => r.UpdateAsync(draftDocument, null), Times.Once);
        }

        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_ShouldReturnDraftDocumentWithoutChangingSubstatus_WhenAnLaExistWithPendingApprovalOrApproved()
        {
            // Arrange
            var userAccount = new UserAccount();
            var draftDocument = new Document
            {
                CABId = "2efe970d-cb83-4f1e-9ced-5489de4af8ca",
                URLSlug = "Test-Kab-17-04-24",
                StatusValue = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new DocumentLegislativeArea { Status = LAStatus.Published},
                    new DocumentLegislativeArea { Status = LAStatus.Declined},
                    new DocumentLegislativeArea { Status = LAStatus.Approved},
                    new DocumentLegislativeArea { Status = LAStatus.PendingApproval},
                },
            };

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument);

            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.PendingApprovalToPublish);
        }    
    }
}
