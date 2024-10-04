using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UKMCAB.Core.Extensions;
using UKMCAB.Data;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Tests.Extensions
{
    [TestFixture]
    public class DocumentExtensionsTests
    {
        [Test]
        public void HasActiveLAs_ReturnsTrue()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new() 
                    {
                        Status = LAStatus.None
                    }
                }
            };

            // Act
            var result = sut.HasActiveLAs();

            // ClassicAssert
            result.Should().BeTrue();
        }

        [Test]
        public void HasActiveLAs_ReturnsFalse()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.DeclinedByOpssAdmin
                    }
                }
            };

            // Act
            var result = sut.HasActiveLAs();

            // ClassicAssert
            result.Should().BeFalse();
        }

        [Test]
        public void IsPendingOgdApproval_ReturnsTrue() 
        {
            // Arrange
            var sut = new Document()
            {
                StatusValue = Status.Draft,
                SubStatus = SubStatus.PendingApprovalToPublish,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.PendingApproval
                    }
                }
            };

            // Act
            var result = sut.IsPendingOgdApproval();

            // ClassicAssert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(IsPendingOgdApprovalInvalidStatuses))]
        public void IsPendingOgdApproval_ReturnsFalse(Status status, SubStatus subStatus, LAStatus laStatus)
        {
            // Arrange
            var sut = new Document()
            {
                StatusValue = status,
                SubStatus = subStatus,
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = laStatus
                    }
                }
            };

            // Act
            var result = sut.IsPendingOgdApproval();

            // ClassicAssert
            result.Should().BeFalse();
        }

        [Test]
        public void LegislativeAreasApprovedByAdminCount_ReturnsNoOfLegislativeAreasApprovedByOpssAdmin()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.Published
                    },
                    new()
                    {
                        Status = LAStatus.None
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreasApprovedByAdminCount();

            // ClassicAssert
            result.Should().Be(1);
        }

        [Test]
        public void LegislativeAreaHasBeenActioned_ReturnsTrue()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.Published
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreaHasBeenActioned();

            // ClassicAssert
            result.Should().BeTrue();
        }

        [Test]
        public void LegislativeAreaHasBeenActioned_ReturnsFalse()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.None
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreaHasBeenActioned();

            // ClassicAssert
            result.Should().BeFalse();
        }

        [Test]
        public void HasActionableLegislativeAreaForOpssAdmin_ReturnsTrue()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.Approved
                    }
                }
            };

            // Act
            var result = sut.HasActionableLegislativeAreaForOpssAdmin();

            // ClassicAssert
            result.Should().BeTrue();
        }

        [Test]
        public void HasActionableLegislativeAreaForOpssAdmin_ReturnsFalse()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.None
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreaHasBeenActioned();

            // ClassicAssert
            result.Should().BeFalse();
        }

        [Test]
        public void LegislativeAreasPendingApprovalByOgd_ReturnsLegislativeAreasPendingApprovalByOgd()
        {
            // Arrange
            var roleId = "Test role";
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.PendingApproval,
                        RoleId = roleId,
                    },
                    new()
                    {
                        Status = LAStatus.PendingApproval,
                    },
                    new()
                    {
                        Status = LAStatus.None,
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreasPendingApprovalByOgd(roleId);

            // ClassicAssert
            result.Should().BeOfType(typeof(List<DocumentLegislativeArea>));
            result.Should().HaveCount(1);
        }

        [Test]
        public void LegislativeAreasPendingApprovalByOpss_ReturnsLegislativeAreasPendingApprovalByOpss()
        {
            // Arrange
            var sut = new Document()
            {
                DocumentLegislativeAreas = new List<DocumentLegislativeArea>
                {
                    new()
                    {
                        Status = LAStatus.Approved,
                    },
                    new()
                    {
                        Status = LAStatus.None,
                    }
                }
            };

            // Act
            var result = sut.LegislativeAreasPendingApprovalByOpss();

            // ClassicAssert
            result.Should().BeOfType(typeof(List<DocumentLegislativeArea>));
            result.Should().HaveCount(1);
        }

        [Test]
        public void LastGovernmentUserNoteDate_ReturnsLatestGovernmentUserNoteDate()
        {
            // Arrange
            var sut = new Document()
            {
                GovernmentUserNotes = new List<UserNote>
                {
                    new()
                    {
                        DateTime = new DateTime(2024, 1, 2)
                    },
                    new()
                    {
                        DateTime = new DateTime(2024, 1, 1)
                    },
                }
            };
            var expectedResult = new DateTime(2024, 1, 2);

            // Act
            var result = sut.LastGovernmentUserNoteDate();

            // ClassicAssert
            result.Should().Be(expectedResult);
        }

        [Test]
        public void LastAuditLogHistoryDate_ReturnsLatestAuditLogDate()
        {
            // Arrange
            var sut = new Document()
            {
                AuditLog = new List<Audit>
                {
                    new()
                    {
                        DateTime = new DateTime(2024, 1, 2)
                    },
                    new()
                    {
                        DateTime = new DateTime(2024, 1, 1)
                    },
                }
            };
            var expectedResult = new DateTime(2024, 1, 2);

            // Act
            var result = sut.LastAuditLogHistoryDate();

            // ClassicAssert
            result.Should().Be(expectedResult);
        }

        [Test]
        public void DraftUpdated_LatestAuditLogCreatedActionDateDiffersFromDocumentLastUpdatedDate_ReturnsTrue()
        {
            // Arrange
            var sut = new Document()
            {
                LastUpdatedDate = new DateTime(2024, 1, 2),
                AuditLog = new List<Audit>
                {
                    new()
                    {
                        Action = AuditCABActions.Created,
                        DateTime = new DateTime(2024, 1, 1)
                    },
                }
            };

            // Act
            var result = sut.DraftUpdated();

            // ClassicAssert
            result.Should().BeTrue();
        }

        [Test]
        public void DraftUpdated_LatestAuditLogCreatedActionDateMatchesDocumentLastUpdatedDate_ReturnsFalse()
        {
            // Arrange
            var sut = new Document()
            {
                LastUpdatedDate = new DateTime(2024, 1, 1),
                AuditLog = new List<Audit>
                {
                    new()
                    {
                        Action = AuditCABActions.Created,
                        DateTime = new DateTime(2024, 1, 1)
                    },
                }
            };

            // Act
            var result = sut.DraftUpdated();

            // ClassicAssert
            result.Should().BeFalse();
        }

        [Test]
        public void PublishedDate_ReturnsFirstAuditLogPublishedActionDate()
        {
            // Arrange
            var sut = new Document()
            {
                AuditLog = new List<Audit>
                {
                    new()
                    {
                        Action = AuditCABActions.Published,
                        DateTime = new DateTime(2024, 1, 1)
                    },
                    new()
                    {
                        Action = AuditCABActions.Published,
                        DateTime = new DateTime(2024, 1, 2)
                    },
                    new()
                    {
                        Action = AuditCABActions.Created,
                        DateTime = new DateTime(2024, 1, 3)
                    }
                }
            };

            // Act
            var result = sut.PublishedDate();

            // ClassicAssert
            result.Should().Be(new DateTime(2024, 1, 1));
        }

        [Test]
        public void PublicDocuments_Returns_Only_Public_Unarchived_Documents()
        {
            var document = new Document()
            {
                Documents = new List<FileUpload>
                {
                    new FileUpload() {
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    }
                }
            };

            var publicDocs = document.PublicDocuments();

            publicDocs.Should().HaveCount(2);
            publicDocs.Any(i => !i.IsPublic).Should().BeFalse();
            publicDocs.Any(i => i.Archived ?? false).Should().BeFalse();
            publicDocs.Any(i => i.Publication == DataConstants.Publications.Private).Should().BeFalse();
        }

        [Test]
        public void HasPublicDocuments_Returns_True_If_Public_Documents()
        {
            var document = new Document()
            {
                Documents = new List<FileUpload>
                {
                    new FileUpload() {
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    }
                }
            };

            var publicDocs = document.HasPublicDocuments();

            publicDocs.Should().BeTrue();
        }

        [Test]
        public void HasPublicDocuments_Returns_Fase_If_No_Public_Documents()
        {
            var document = new Document()
            {
                Documents = new List<FileUpload>
                {
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    },
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Private
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    }
                }
            };

            var publicDocs = document.HasPublicDocuments();

            publicDocs.Should().BeFalse();
        }

        [Test]
        public void HasPublicDocuments_Returns_Fase_If_Public_Documents_Are_Archived()
        {
            var document = new Document()
            {
                Documents = new List<FileUpload>
                {
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Archived = true,
                        Publication = DataConstants.Publications.Public
                    },
                    new FileUpload() {
                        Publication = DataConstants.Publications.Private
                    }
                }
            };

            var publicDocs = document.HasPublicDocuments();

            publicDocs.Should().BeFalse();
        }


        private static IEnumerable<TestCaseData> IsPendingOgdApprovalInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(Status)).Cast<Status>().Where(s => s is not Status.Draft);
                foreach (var status in invalidStatuses)
                {
                    yield return new TestCaseData(status, SubStatus.PendingApprovalToPublish, LAStatus.PendingApproval);
                }
                var invalidSubStatuses = Enum.GetValues(typeof(SubStatus)).Cast<SubStatus>().Where(s => s is not SubStatus.PendingApprovalToPublish);
                foreach (var status in invalidSubStatuses)
                {
                    yield return new TestCaseData(Status.Draft, status, LAStatus.PendingApproval);
                }
                yield return new TestCaseData(Status.Draft, SubStatus.PendingApprovalToPublish, LAStatus.None);
            }
        }
    }
}
