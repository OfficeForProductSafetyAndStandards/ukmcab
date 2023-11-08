using System.ComponentModel;

namespace UKMCAB.Core.Domain.Workflow;

public enum TaskType
{
    [Description("Approve CAB request")]
    RequestToPublish,
    [Description("Unarchive CAB request")]
    RequestToUnarchive,
    [Description("CAB review due")]
    ReviewCAB,
    [Description("Approve account request")]
    UserAccountRequest,
    [Description("Back to UKAS with CAB Published - (completed task)")]
    CABPublished,
    [Description("Back to UKAS with CAB Declined - (completed task)")]
    CABDeclined,
    [Description("Approved Unarchive to UKAS (Completed)")]
    UnarchiveApproved,
    [Description("Declined Unarchive to UKAS (Completed)")]
    UnarchiveDeclined,
    [Description("User account request approval back to UKAS/OPSS user(Completed)")]
    UserAccountApproved
}