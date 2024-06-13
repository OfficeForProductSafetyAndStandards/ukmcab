using System.ComponentModel;

namespace UKMCAB.Data.Models
{
    public enum Status
    {
        Unknown = 0,
        Draft = 20,
        Published = 30,
        Archived = 40,
        Historical = 50,
    }

    public enum SubStatus
    {
        [Description("None")]
        None,
        [Description("Pending approval to publish CAB")]
        PendingApprovalToPublish,
        [Description("Pending approval to archive CAB")]
        PendingApprovalToArchive,
        [Description("Pending approval to unarchive and publish CAB")]
        PendingApprovalToUnarchivePublish,
        [Description("Pending approval to unarchive CAB as draft")]
        PendingApprovalToUnarchive,
        [Description("Pending approval to unpublish CAB")]
        PendingApprovalToUnpublish,
    }
    public enum LAStatus
    {
        [Description("None")]
        None,
        [Description("Approved")]
        Approved,
        [Description("Declined")]
        Declined,
        [Description("Draft")]
        Draft,
        [Description("Published")]
        Published,
        [Description("Pending approval")]
        PendingApproval,       
        [Description("Approved by OPSS")]
        ApprovedByOpssAdmin,
        [Description("Declined by OPSS")]
        DeclinedByOpssAdmin,
        [Description("Approved by OPSS")]
        ApprovedToRemoveByOpssAdmin,
        [Description("Approved by OPSS")]
        ApprovedToArchiveAndRemoveScheduleByOpssAdmin,
        [Description("Approved by OPSS")]
        ApprovedToArchiveAndArchiveScheduleByOpssAdmin,
        [Description("To remove")]
        PendingSubmissionToRemove,
        [Description("Pending approval")]
        PendingApprovalToRemove,
        [Description("Pending approval")]
        PendingApprovalToRemoveByOpssAdmin,
        [Description("To archive")]
        PendingSubmissionToArchiveAndArchiveSchedule, // UKAS to OGD approval before submission
        [Description("Pending approval")]
        PendingApprovalToArchiveAndArchiveSchedule,  // After UKAS submitted the LA archive request and waiting for OGD approval
        [Description("Pending approval")]
        PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin,
        [Description("To archive")]
        PendingSubmissionToArchiveAndRemoveSchedule, // UKAS to OGD approval before submission
        [Description("Pending approval")]
        PendingApprovalToArchiveAndRemoveSchedule,  // After UKAS submitted the LA archive request and waiting for OGD approval              
        [Description("Pending approval")]
        PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin,
        [Description("To unarchive")]
        PendingSubmissionToUnarchive,
        [Description("Pending approval")]
        PendingApprovalToUnarchive,
        [Description("Declined")]
        DeclinedToRemoveByOGD,
        [Description("Declined by OPSS")]
        DeclinedToRemoveByOPSS,
        [Description("Pending approval")]
        PendingApprovalToUnarchiveByOpssAdmin,
        [Description("Declined")]
        DeclinedToUnarchiveByOGD,
        [Description("Declined by OPSS")]
        DeclinedToUnarchiveByOPSS,
        [Description("Approved by OPSS")]
        ApprovedToUnarchiveByOPSS,
        [Description("Declined")]
        DeclinedToArchiveAndArchiveScheduleByOGD,
        [Description("Declined by OPSS")]
        DeclinedToArchiveAndArchiveScheduleByOPSS,
        [Description("Declined")]
        DeclinedToArchiveAndRemoveScheduleByOGD,
        [Description("Declined by OPSS")]
        DeclinedToArchiveAndRemoveScheduleByOPSS
    }
    public class LAStatusCategory
    {
        public static readonly List<string> ApprovedByOGD = new()
        {
            ((int)LAStatus.Approved).ToString()
        };
        public static readonly List<string> Draft = new()
        {
            ((int)LAStatus.Draft).ToString()
        };
        public static readonly List<string> Published = new()
        {
            ((int)LAStatus.Published).ToString()
        };
        public static readonly List<string> DeclinedByOGD = new()
        {
            ((int)LAStatus.Declined).ToString(),
            ((int)LAStatus.DeclinedToRemoveByOGD).ToString(),
            ((int)LAStatus.DeclinedToUnarchiveByOGD).ToString(),
            ((int)LAStatus.DeclinedToArchiveAndArchiveScheduleByOGD).ToString(),
            ((int)LAStatus.DeclinedToArchiveAndRemoveScheduleByOGD).ToString(),
        };
        public static readonly List<string> PendingOGDApproval = new()
        {
            ((int)LAStatus.PendingApproval).ToString(),
            ((int)LAStatus.PendingApprovalToRemove).ToString(),
            ((int)LAStatus.PendingApprovalToRemove).ToString(),
            ((int)LAStatus.PendingApprovalToArchiveAndArchiveSchedule).ToString(),
            ((int)LAStatus.PendingApprovalToArchiveAndRemoveSchedule).ToString(),
            ((int)LAStatus.PendingApprovalToUnarchive).ToString(),
        };
        public static readonly List<string> ApprovedByOPSS = new()
        {
            ((int)LAStatus.ApprovedByOpssAdmin).ToString(),
            ((int)LAStatus.ApprovedToRemoveByOpssAdmin).ToString(),
            ((int)LAStatus.ApprovedToArchiveAndRemoveScheduleByOpssAdmin).ToString(),
            ((int)LAStatus.ApprovedToArchiveAndArchiveScheduleByOpssAdmin).ToString(),
            ((int)LAStatus.ApprovedToUnarchiveByOPSS).ToString(),
        };
        public static readonly List<string> DeclinedByOPSS = new()
        {
            ((int)LAStatus.DeclinedByOpssAdmin).ToString(),
            ((int)LAStatus.DeclinedToRemoveByOPSS).ToString(),
            ((int)LAStatus.DeclinedToUnarchiveByOPSS).ToString(),
            ((int)LAStatus.DeclinedToArchiveAndArchiveScheduleByOPSS).ToString(),
            ((int)LAStatus.DeclinedToArchiveAndRemoveScheduleByOPSS).ToString(),
        };
        public static readonly List<string> PendingOPSSApproval = new()
        {
            ((int)LAStatus.PendingApprovalToRemoveByOpssAdmin).ToString(),
            ((int)LAStatus.PendingApprovalToArchiveAndArchiveScheduleByOpssAdmin).ToString(),
            ((int)LAStatus.PendingApprovalToArchiveAndRemoveScheduleByOpssAdmin).ToString(),
            ((int)LAStatus.PendingApprovalToUnarchiveByOpssAdmin).ToString(),
        };
        public static readonly List<string> PendingUKASSubmission = new()
        {
            ((int)LAStatus.PendingSubmissionToRemove).ToString(),
            ((int)LAStatus.PendingSubmissionToArchiveAndArchiveSchedule).ToString(),
            ((int)LAStatus.PendingSubmissionToArchiveAndRemoveSchedule).ToString(),
            ((int)LAStatus.PendingSubmissionToUnarchive).ToString(),
        };
    }
}
