using System.ComponentModel;

namespace UKMCAB.Core.Domain.Workflow;

public enum TaskType
{
    [Description("Approve CAB request")]
    RequestToPublish,
    [Description("Unarchive CAB request")]
    RequestToUnarchiveForDraft,
    [Description("Unarchive CAB request")]
    RequestToUnarchiveForPublish,
    [Description("CAB review due")]
    ReviewCAB,
    [Description("Approve account request")]
    UserAccountRequest,
    [Description("CAB approved")]
    CABPublished,
    [Description("CAB declined")]
    CABDeclined,
    UnarchiveApproved,
    UnarchiveDeclined,
    UserAccountApproved
}