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
        [Description("To archive and archive product schedule")]
        PendingSubmissionToArchiveAndArchiveSchedule,
        [Description("Pending approval")]
        PendingApprovalToArchiveAndArchiveSchedule,
        [Description("Pending approval")]
        PendingApprovalToToArchiveAndArchiveScheduleByOpssAdmin,
        [Description("To archive and remove product schedule")]
        PendingSubmissionToArchiveAndRemoveSchedule,
        [Description("Pending approval")]
        PendingApprovalToArchiveAndRemoveSchedule,               
        [Description("Pending approval")]
        PendingApprovalToToArchiveAndRemoveScheduleByOpssAdmin,
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
        DeclinedToUnarchiveByOPSS
    }
}
