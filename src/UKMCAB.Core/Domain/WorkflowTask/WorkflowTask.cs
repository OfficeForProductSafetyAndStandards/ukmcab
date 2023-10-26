namespace UKMCAB.Core.Domain.WorkflowTask;

public record WorkflowTask(string Id, TaskType TaskType, TaskState State, User Submitter, User Assignee, DateTime? Assigned,
    string Reason, DateTime SentOn, User LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid documentId);