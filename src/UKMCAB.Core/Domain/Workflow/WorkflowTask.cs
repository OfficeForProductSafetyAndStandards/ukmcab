namespace UKMCAB.Core.Domain.Workflow;

public record WorkflowTask(Guid Id, TaskType TaskType, User Submitter, string ForRoleId, User? Assignee, DateTime? Assigned,
    string Body, DateTime SentOn, User LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid? CABId = null)
{
    // set properties
    public User? Assignee { get; set; } = Assignee;
    public DateTime? Assigned { get; set; } = Assigned;
    public string Body { get; set; } = Body;
    public User LastUpdatedBy { get; set; } = LastUpdatedBy;
    public DateTime LastUpdatedOn { get; set; } = LastUpdatedOn;
    public bool? Approved { get; set; } = Approved;
    public string? DeclineReason { get; set; } = DeclineReason;
    public bool Completed { get; set; } = Completed;
    public Guid? CABId { get; set; } = CABId;
}