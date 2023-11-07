using System.ComponentModel;

namespace UKMCAB.Core.Domain.Workflow;

public enum TaskType
{
    [Description("Request to publish")]
    RequestToPublish,
    [Description("Request to unarchive")]
    RequestToUnarchive,
    [Description("Review CAB")]
    ReviewCAB,
    [Description("User account request")]
    UserAccountRequest
}