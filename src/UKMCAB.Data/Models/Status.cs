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
        [Description("Published")]
        Published,
        [Description("Pending approval from DFTP")]
        PendingApprovalFromDFTP,
        [Description("Pending approval from DFTR")]
        PendingApprovalFromDFTR,
        [Description("Pending approval from DLUHC")]
        PendingApprovalFromDLUHC,
        [Description("Pending approval from MCGA")]
        PendingApprovalFromMCGA,
        [Description("Pending approval from MHRA")]
        PendingApprovalFromMHRA,
        [Description("Pending approval from OPSS (OGD)")]
        PendingApprovalFromOPSS_OGD
    }
}
