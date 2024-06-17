namespace UKMCAB.Core.Domain.Workflow;

public record WorkflowTask(
    TaskType TaskType,
    User Submitter,
    string ForRoleId,
    User? Assignee,
    DateTime? Assigned,
    string Body,
    User LastUpdatedBy,
    DateTime LastUpdatedOn,
    bool? Approved,
    string? DeclineReason,
    bool Completed,
    Guid? CABId = null,
    Guid? DocumentLAId = null)
{
    // set properties
    public Guid Id { get; set; } = Guid.NewGuid();
    public User? Assignee { get; set; } = Assignee;
    public DateTime? Assigned { get; set; } = Assigned;
    public string Body { get; set; } = Body;
    public User LastUpdatedBy { get; set; } = LastUpdatedBy;
    public DateTime LastUpdatedOn { get; set; } = LastUpdatedOn;
    public bool? Approved { get; set; } = Approved;
    public bool Completed { get; set; } = Completed;
    public DateTime SentOn { get; set; } = DateTime.Now;
}