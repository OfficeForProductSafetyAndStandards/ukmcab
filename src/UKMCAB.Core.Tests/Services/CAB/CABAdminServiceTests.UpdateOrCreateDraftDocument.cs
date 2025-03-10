﻿using Moq;
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
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new DocumentLegislativeArea { Status = LAStatus.Draft, NewlyCreated = true },
                    new DocumentLegislativeArea { Status = LAStatus.Draft, NewlyCreated = false },
                    new DocumentLegislativeArea { Status = LAStatus.Draft},
                },

            };

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument, submitForApproval);

            // Assert
            Assert.That(result.SubStatus, Is.EqualTo(SubStatus.PendingApprovalToPublish));
            Assert.That(result.AuditLog.First().Action, Is.EqualTo(AuditCABActions.SubmittedForApproval));
            Assert.That(result.DocumentLegislativeAreas.All(x => x.Status == LAStatus.PendingApproval && x.NewlyCreated == null), Is.True);
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
            Assert.That(intermediateDocument.StatusValue, Is.EqualTo(Status.Draft));
            Assert.That(intermediateDocument.AuditLog.First().Action, Is.EqualTo(AuditCABActions.Created));
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
            Assert.That(result.SubStatus, Is.EqualTo(SubStatus.None));
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
            Assert.That(result.SubStatus, Is.EqualTo(SubStatus.PendingApprovalToPublish));
        }    
    }
}
