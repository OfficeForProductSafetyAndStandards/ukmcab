using NUnit.Framework;
using System.Collections.Generic;
using UKMCAB.Core.Extensions;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

public static class CABSummaryViewModelTestsHelpers
{
    public static IEnumerable<TestCaseData> GetTestCases()
    {
        var doc = new Document { StatusValue = Status.Draft, SubStatus = SubStatus.PendingApprovalToPublish};
        var docLA = doc.DocumentLegislativeAreas;
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Approved });

        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true);

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Declined });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclined_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToRemoveByOPSS });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToRemoveByOPSS_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsApprovedByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToRemoveByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToRemoveByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToRemoveByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToRemoveByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToArchiveAndArchiveScheduleByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToArchiveAndRemoveScheduleByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndArchiveScheduleByOpssAdmin_ShouldBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndRemoveScheduleByOpssAdmin");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndArchiveScheduleByOGD");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndArchiveScheduleByOPSS");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndRemoveScheduleByOGD");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToArchiveAndRemoveScheduleByOPSS");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.ApprovedToUnarchiveByOPSS });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsApprovedToUnarchiveByOPSS");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToUnarchiveByOpssAdmin });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsPendingApprovalToUnarchiveByOpssAdmin");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.DeclinedToUnarchiveByOPSS });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDeclinedToUnarchiveByOPSS");


        // Sad Path
        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApproval });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, false).SetName("Test_WhenLAStatusIsPendingApproval_ShouldNotBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Draft });
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.Published });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsDraftAndPublished_ShouldNotBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToUnarchive });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToUnarchive_ShouldNotBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToArchiveAndArchiveSchedule });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToArchiveAndArchiveSchedule_ShouldNotBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.None });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, true).SetName("Test_WhenLAStatusIsNone_ShouldNotBeActionable");

        docLA.Clear();
        docLA.Add(new DocumentLegislativeArea { Status = LAStatus.PendingApprovalToRemove });
        yield return new TestCaseData(true, false, Status.Draft, SubStatus.PendingApprovalToPublish, doc.IsPendingOgdApproval(), false, false, false).SetName("Test_WhenLAStatusIsPendingApprovalToRemove_ShouldNotBeActionable");
    }

    public static void SetShowSubSectionEditActionToTrueOpssAdmin(CABSummaryViewModel cabSummary)
    {
        cabSummary.RevealEditActions = true;
        cabSummary.IsEditLocked = false;
        cabSummary.SubStatus = SubStatus.PendingApprovalToPublish;
        cabSummary.IsOpssAdmin = true;
        cabSummary.HasActionableLegislativeAreaForOpssAdmin = true;
    }

    public static void SetCanPublishToTrueOpssAdmin(CABSummaryViewModel cabSummary)
    {        
        cabSummary.IsOpssAdmin = true;
        cabSummary.DraftUpdated = true;
        cabSummary.LegislativeAreasApprovedByAdminCount = 1;
    }

    public static void SetValidCABToTrueOpssAdmin(CABSummaryViewModel cabSummary)
    {
        cabSummary.Status = Status.Draft;
        cabSummary.LegislativeAreasApprovedByAdminCount = 1;
        SetIsCompleteToTrue(cabSummary);
        cabSummary.HasActiveLAs = true;
    }

    public static void SetIsCompleteToTrue(CABSummaryViewModel cabSummary)
    { 
        cabSummary.CabDetailsViewModel = new CABDetailsViewModel { IsCompleted = true };
        cabSummary.CabContactViewModel = new CABContactViewModel { IsCompleted = true };
        cabSummary.CabBodyDetailsViewModel = new CABBodyDetailsViewModel { IsCompleted = true };
        cabSummary.CabLegislativeAreasViewModel = new CABLegislativeAreasViewModel { IsCompleted = true };
        cabSummary.CABProductScheduleDetailsViewModel = new CABProductScheduleDetailsViewModel { IsCompleted = true };
        cabSummary.CABSupportingDocumentDetailsViewModel = new CABSupportingDocumentDetailsViewModel { IsCompleted = true };
        cabSummary.CABHistoryViewModel = new CABHistoryViewModel { IsCompleted = true };
        cabSummary.CABGovernmentUserNotesViewModel = new CABGovernmentUserNotesViewModel { IsCompleted = true };
    }

    public static void SetShowOgdActionsToTrue(CABSummaryViewModel cabSummary)
    {
        cabSummary.HasOgdRole = true;
        cabSummary.IsPendingOgdApproval = true;
        cabSummary.RevealEditActions = true;
        cabSummary.IsEditLocked = false;
        cabSummary.LegislativeAreasPendingApprovalForCurrentUserCount = 1;
    }

    public static void SetEditByGroupPermittedToTrue(CABSummaryViewModel cabSummary)
    {
        cabSummary.SubStatus = SubStatus.None;
        cabSummary.Status = Status.Published;
        cabSummary.UserInCreatorUserGroup = true;
    }

    public static void SetEditByGroupPermittedToFalse(CABSummaryViewModel cabSummary)
    {
        cabSummary.SubStatus = SubStatus.None;
        cabSummary.Status = Status.Draft;
        cabSummary.UserInCreatorUserGroup = false;
    }
}