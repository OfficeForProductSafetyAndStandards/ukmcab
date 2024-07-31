using Bogus;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UKMCAB.Core.Extensions;
using UKMCAB.Data.Models;

namespace UKMCAB.Core.Tests.Extensions
{
    [TestFixture]
    public class DocumentLegislativeAreaExtensionsTests
    {
        [TestCase(LAStatus.Published)]
        [TestCase(LAStatus.ApprovedByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToRemoveByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToUnarchiveByOPSS)]
        public void ApprovedByOpssAdmin_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.ApprovedByOpssAdmin();

            // Assert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(ApprovedByOpssAdminInvalidStatuses))]
        public void ApprovedByOpssAdmin_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.ApprovedByOpssAdmin();

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(LAStatus.PendingApproval)]
        [TestCase(LAStatus.PendingApprovalToRemove)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndArchiveSchedule)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndRemoveSchedule)]
        [TestCase(LAStatus.PendingApprovalToUnarchive)]
        public void IsPendingOgdApproval_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsPendingOgdApproval();

            // Assert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(IsPendingOgdApprovalInvalidStatuses))]
        public void IsPendingOgdApproval_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsPendingOgdApproval();

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(LAStatus.Approved)]
        [TestCase(LAStatus.PendingApprovalToRemoveByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToUnarchive)]
        [TestCase(LAStatus.PendingApprovalToUnarchiveByOpssAdmin)]
        public void IsPendingApprovalByOpss_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsPendingApprovalByOpss();

            // Assert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(IsPendingApprovalByOpssInvalidStatuses))]
        public void IsPendingApprovalByOpss_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsPendingApprovalByOpss();

            // Assert
            result.Should().BeFalse();
        }

        [TestCaseSource(nameof(IsActiveValidStatuses))]
        public void IsActive_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsActive();

            // Assert
            result.Should().BeTrue();
        }

        [TestCase(LAStatus.DeclinedByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToRemoveByOpssAdmin)]
        public void IsActive_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.IsActive();

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(LAStatus.Approved)]
        [TestCase(LAStatus.Declined)]
        [TestCase(LAStatus.DeclinedToRemoveByOPSS)]
        [TestCase(LAStatus.ApprovedByOpssAdmin)]
        [TestCase(LAStatus.DeclinedByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToRemoveByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToRemoveByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD)]
        [TestCase(LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS)]
        [TestCase(LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD)]
        [TestCase(LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS)]
        [TestCase(LAStatus.ApprovedToUnarchiveByOPSS)]
        [TestCase(LAStatus.PendingApprovalToUnarchiveByOpssAdmin)]
        [TestCase(LAStatus.DeclinedToUnarchiveByOPSS)]
        public void ActionableByOpssAdmin_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.ActionableByOpssAdmin();

            // Assert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(ActionableByOpssAdminInvalidStatuses))]
        public void ActionableByOpssAdmin_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.ActionableByOpssAdmin();

            // Assert
            result.Should().BeFalse();
        }

        [TestCase(LAStatus.Published)]
        [TestCase(LAStatus.Approved)]
        [TestCase(LAStatus.Declined)]
        [TestCase(LAStatus.DeclinedToRemoveByOPSS)]
        [TestCase(LAStatus.ApprovedByOpssAdmin)]
        [TestCase(LAStatus.DeclinedByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToRemoveByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToRemoveByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin)]
        [TestCase(LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin)]
        [TestCase(LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD)]
        [TestCase(LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS)]
        [TestCase(LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD)]
        [TestCase(LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS)]
        [TestCase(LAStatus.ApprovedToUnarchiveByOPSS)]
        [TestCase(LAStatus.PendingApprovalToUnarchiveByOpssAdmin)]
        [TestCase(LAStatus.DeclinedToUnarchiveByOPSS)]
        public void HasBeenActioned_ReturnsTrue(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.HasBeenActioned();

            // Assert
            result.Should().BeTrue();
        }

        [TestCaseSource(nameof(HasBeenActionedInvalidStatuses))]
        public void HasBeenActioned_ReturnsFalse(LAStatus status)
        {
            // Arrange
            var sut = new DocumentLegislativeArea
            {
                Status = status
            };

            // Act
            var result = sut.HasBeenActioned();

            // Assert
            result.Should().BeFalse();
        }

        [TestCaseSource(typeof(MarkAsDraftTestData), nameof(MarkAsDraftTestData.TestCases))]
        public LAStatus MarkAsDraft_DocumentStatusValueIsDraftAndSubStatusIsNoneAndLegislativeAreaIsPublishedOrDeclined(LAStatus laStatus, Status documentStatus, SubStatus documentSubStatus)
        {
            // Arrange
            var documentLegislativeArea = new DocumentLegislativeArea
            {
                Status = laStatus
            };

            // Act
            documentLegislativeArea.MarkAsDraft(documentStatus, documentSubStatus);

            // Assert
            return documentLegislativeArea.Status;
        }

        private static IEnumerable<LAStatus> ApprovedByOpssAdminInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.Published
                    and not LAStatus.ApprovedByOpssAdmin
                    and not LAStatus.ApprovedToRemoveByOpssAdmin
                    and not LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin
                    and not LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin
                    and not LAStatus.ApprovedToUnarchiveByOPSS);
                foreach (var status in invalidStatuses)
                {
                    yield return status;
                }
            }
        }

        private static IEnumerable<LAStatus> IsPendingOgdApprovalInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.PendingApproval
                    and not LAStatus.PendingApprovalToRemove
                    and not LAStatus.PendingApprovalToArchiveAndArchiveSchedule
                    and not LAStatus.PendingApprovalToArchiveAndRemoveSchedule
                    and not LAStatus.PendingApprovalToUnarchive);
                foreach (var status in invalidStatuses)
                {
                    yield return status;
                }
            }
        }

        private static IEnumerable<LAStatus> IsPendingApprovalByOpssInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.Approved
                    and not LAStatus.PendingApprovalToRemoveByOpssAdmin
                    and not LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin
                    and not LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin
                    and not LAStatus.PendingApprovalToUnarchive
                    and not LAStatus.PendingApprovalToUnarchiveByOpssAdmin);
                foreach (var status in invalidStatuses)
                {
                    yield return status;
                }
            }
        }

        private static IEnumerable<LAStatus> IsActiveValidStatuses
        {
            get
            {
                var validStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.DeclinedByOpssAdmin
                    and not LAStatus.ApprovedToRemoveByOpssAdmin);
                foreach (var status in validStatuses)
                {
                    yield return status;
                }
            }
        }

        private static IEnumerable<LAStatus> ActionableByOpssAdminInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.Approved and not
                    LAStatus.Declined and not
                    LAStatus.DeclinedToRemoveByOPSS and not
                    LAStatus.ApprovedByOpssAdmin and not
                    LAStatus.DeclinedByOpssAdmin and not
                    LAStatus.PendingApprovalToRemoveByOpssAdmin and not
                    LAStatus.ApprovedToRemoveByOpssAdmin and not
                    LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin and not
                    LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin and not
                    LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin and not
                    LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin and not
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD and not
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS and not
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD and not
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS and not
                    LAStatus.ApprovedToUnarchiveByOPSS and not
                    LAStatus.PendingApprovalToUnarchiveByOpssAdmin and not
                    LAStatus.DeclinedToUnarchiveByOPSS);
                foreach (var status in invalidStatuses)
                {
                    yield return status;
                }
            }
        }

        private static IEnumerable<LAStatus> HasBeenActionedInvalidStatuses
        {
            get
            {
                var invalidStatuses = Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s =>
                    s is not LAStatus.Published and not
                    LAStatus.Approved and not
                    LAStatus.Declined and not
                    LAStatus.DeclinedToRemoveByOPSS and not
                    LAStatus.ApprovedByOpssAdmin and not
                    LAStatus.DeclinedByOpssAdmin and not
                    LAStatus.PendingApprovalToRemoveByOpssAdmin and not
                    LAStatus.ApprovedToRemoveByOpssAdmin and not
                    LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin and not
                    LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin and not
                    LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin and not
                    LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin and not
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD and not
                    LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS and not
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD and not
                    LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS and not
                    LAStatus.ApprovedToUnarchiveByOPSS and not
                    LAStatus.PendingApprovalToUnarchiveByOpssAdmin and not
                    LAStatus.DeclinedToUnarchiveByOPSS);
                foreach (var status in invalidStatuses)
                {
                    yield return status;
                }
            }
        }

        public class MarkAsDraftTestData
        {
            public static List<LAStatus> ValidLaStatusesForTransitionToDraft = new List<LAStatus> { LAStatus.Published, LAStatus.Declined, LAStatus.DeclinedByOpssAdmin };

            public static IEnumerable TestCases
            {
                get
                {
                    yield return new TestCaseData(LAStatus.Published, Status.Draft, SubStatus.None).Returns(LAStatus.Draft);
                    yield return new TestCaseData(LAStatus.Declined, Status.Draft, SubStatus.None).Returns(LAStatus.Draft);
                    yield return new TestCaseData(LAStatus.DeclinedByOpssAdmin, Status.Draft, SubStatus.None).Returns(LAStatus.Draft);

                    foreach (var documentStatus in Enum.GetValues(typeof(Status)).Cast<Status>().Where(s => s != Status.Draft))
                    {
                        yield return new TestCaseData(LAStatus.Published, documentStatus, SubStatus.None).Returns(LAStatus.Published);
                    }

                    foreach (var documentSubStatus in Enum.GetValues(typeof(SubStatus)).Cast<SubStatus>().Where(s => s != SubStatus.None))
                    {
                        yield return new TestCaseData(LAStatus.Published, Status.Draft, documentSubStatus).Returns(LAStatus.Published);
                    }

                    foreach (var laStatus in Enum.GetValues(typeof(LAStatus)).Cast<LAStatus>().Where(s => !ValidLaStatusesForTransitionToDraft.Contains(s)))
                    {
                        yield return new TestCaseData(laStatus, Status.Draft, SubStatus.None).Returns(laStatus);
                    }
                }
            }
        }
    }
}
