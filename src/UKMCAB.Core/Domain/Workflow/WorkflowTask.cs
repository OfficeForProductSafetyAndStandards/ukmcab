using UKMCAB.Core.Security;

namespace UKMCAB.Core.Domain.Workflow;

public record WorkflowTask(Guid Id, TaskType TaskType, TaskState State, User Submitter, string ForRoleId, User? Assignee, DateTime? Assigned,
    string Reason, DateTime SentOn, User LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid CABId)
{
    // set properties
    public User? Assignee { get; set; } = Assignee;
    
    public DateTime? Assigned { get; set; } = Assigned;
    public TaskState State { get; set; } = State;
    public string Reason { get; set; } = Reason;
    public User LastUpdatedBy { get; set; } = LastUpdatedBy;
    public DateTime LastUpdatedOn { get; set; } = LastUpdatedOn;
    public bool? Approved { get; set; } = Approved;
    public string? DeclineReason { get; set; } = DeclineReason;
    public bool Completed { get; set; } = Completed;
}