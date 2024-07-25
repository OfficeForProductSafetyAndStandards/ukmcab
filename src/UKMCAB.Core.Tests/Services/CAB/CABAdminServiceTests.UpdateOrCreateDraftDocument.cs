using Bogus.DataSets;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UKMCAB.Core.Security;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using static System.Net.WebRequestMethods;

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

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument, submitForApproval);
            
            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.PendingApprovalToPublish);
            Assert.AreEqual(result.AuditLog.First().Action, AuditCABActions.SubmittedForApproval);
            Assert.IsTrue(result.DocumentLegislativeAreas.All(x => x.Status == LAStatus.PendingApproval));
            _mockCABRepository.Verify(r => r.UpdateAsync(draftDocument), Times.Once);
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
            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());
            
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

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument);

            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.None);
            _mockCABRepository.Verify(r => r.UpdateAsync(draftDocument), Times.Once);
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

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>());
            
            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, draftDocument);

            // Assert
            Assert.AreEqual(result.SubStatus, SubStatus.PendingApprovalToPublish);
        }

        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_DocumentIsPublishedAndExistingDocumentIsDifferentFromCurrent_CreatesNewDraft()
        {
            // Arrange
            var userAccount = new UserAccount
            {
                Role = Roles.DFTP.Id
            };
            var document = new Document
            {
                id = "Test id",
                StatusValue = Status.Published,
                Name = "New test name",
            };

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>
            {
                new()
                {
                    id = "Test id",
                    StatusValue = Status.Published,
                    Name = "Test name"
                }
            });
            _mockCABRepository.Setup(x => x.CreateAsync(It.IsAny<Document>(), It.IsAny<DateTime>())).ReturnsAsync(new Document());

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, document);

            // Assert
            _mockCABRepository.Verify(x => x.CreateAsync(
                It.Is<Document>(d =>
                    d.id == document.id &&
                    d.StatusValue == Status.Draft &&
                    d.Name == "New test name" &&
                    d.CreatedByUserGroup == Roles.DFTP.Id &&
                    d.AuditLog.Count == 1 &&
                    d.AuditLog.First().Action == AuditCABActions.Created),
                It.IsAny<DateTime>()), Times.Once);
        }

        [TestCaseSource(nameof(UpdateOrCreateDraftDocumentAsync_DocumentNotPublishedTestCases))]
        public async Task UpdateOrCreateDraftDocumentAsync_DocumentNotPublished_DoesNotCreateNewDraft(Document document)
        {
            // Arrange
            var userAccount = new UserAccount
            {
                Role = Roles.DFTP.Id
            };

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>
            {
                new()
                {
                    id = "Test id",
                    StatusValue = Status.Draft,
                    Name = "Test name"
                }
            });

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, document);

            // Assert
            _mockCABRepository.Verify(x => x.CreateAsync(It.IsAny<Document>(),It.IsAny<DateTime>()), Times.Never);
        }

        [Test]
        public async Task UpdateOrCreateDraftDocumentAsync_CurrentDocumentSameAsExisting_DoesNotCreateNewDraft()
        {
            // Arrange
            var userAccount = new UserAccount
            {
                Role = Roles.DFTP.Id
            };
            var document = new Document
            {
                id = "Test id",
                StatusValue = Status.Published,
                Name = "Test name",
            };

            _mockCABRepository.Setup(x => x.Query(It.IsAny<Expression<Func<Document, bool>>>())).ReturnsAsync(new List<Document>
            {
                new()
                {
                    id = "Test id",
                    StatusValue = Status.Published,
                    Name = "Test name"
                }
            });

            // Act 
            var result = await _sut.UpdateOrCreateDraftDocumentAsync(userAccount, document);

            // Assert
            _mockCABRepository.Verify(x => x.CreateAsync(It.IsAny<Document>(), It.IsAny<DateTime>()), Times.Never);
        }

        private static IEnumerable<Document> UpdateOrCreateDraftDocumentAsync_DocumentNotPublishedTestCases()
        {
            foreach (var status in Enum.GetValues(typeof(Status)).Cast<Status>().Where(s => s != Status.Published))
            {
                yield return new Document
                {
                    id = "Test id",
                    StatusValue = status,
                    Name = "New test name",
                };
            }
        }
    }
}
