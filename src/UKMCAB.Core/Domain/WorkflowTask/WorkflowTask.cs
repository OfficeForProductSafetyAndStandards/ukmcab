namespace UKMCAB.Core.Domain.WorkflowTask;

public record WorkflowTask
{
    public WorkflowTask(Guid Id, TaskType TaskType, TaskState State, User Submitter, User? Assignee, DateTime? Assigned,
        string Reason, DateTime SentOn, User LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
        string? DeclineReason, bool Completed, Guid DocumentId)
    {
        this.Id = Id;
        this.TaskType = TaskType;
        this.State = State;
        this.Submitter = Submitter;
        this.Assignee = Assignee;
        this.Assigned = Assigned;
        this.Reason = Reason;
        this.SentOn = SentOn;
        this.LastUpdatedBy = LastUpdatedBy;
        this.LastUpdatedOn = LastUpdatedOn;
        this.Approved = Approved;
        this.DeclineReason = DeclineReason;
        this.Completed = Completed;
        this.DocumentId = DocumentId;
    }

    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public TaskType TaskType { get; init; }
    public User Submitter { get; init; }
    public DateTime SentOn { get; init; }
    public User? Assignee { get; set; }
    public DateTime? Assigned { get; set; }
    public TaskState State { get; set; }
    public string Reason { get; set; }
    public User LastUpdatedBy { get; set; }
    public DateTime LastUpdatedOn { get; set; }
    public bool? Approved { get; set; }
    public string? DeclineReason { get; set; }
    public bool Completed { get; set; }
}