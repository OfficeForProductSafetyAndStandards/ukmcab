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
    CABPublished,
    CABDeclined,
    UnarchiveApproved,
    UnarchiveDeclined,
    UserAccountApproved
}