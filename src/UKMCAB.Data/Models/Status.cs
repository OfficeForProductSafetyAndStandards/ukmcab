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
        [Description("Pending approval to publish")]
        PendingApprovalToPublish,
        [Description("Pending approval to archive")]
        PendingApprovalToArchive,
        [Description("Pending approval to publish")]
        PendingApprovalToUnarchivePublish,
        [Description("Pending approval to unarchive")]
        PendingApprovalToUnarchive
    }
}
