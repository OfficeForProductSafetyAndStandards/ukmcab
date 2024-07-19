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
        [TestCaseSource(typeof(DocumentLegislativeAreaMarkAsDraftData), nameof(DocumentLegislativeAreaMarkAsDraftData.TestCases))]
        public LAStatus ShouldUpdateStatusToDraft_When_DocumentStatusValueIsDraft_And_DocumentSubStatusIsNone_And_DocumentLegislativeAreaIsPublishedOrDeclined(LAStatus laStatus, Status documentStatus, SubStatus documentSubStatus)
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
    }

    public class DocumentLegislativeAreaMarkAsDraftData
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
