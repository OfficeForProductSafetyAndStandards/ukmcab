using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models.WorkflowTask;

public abstract record WorkflowTask(string Id, TaskType TaskType, TaskState State, UserAccount Submitter, UserAccount Assignee, DateTime? Assigned,
    string Reason, DateTime SentOn, UserAccount LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid documentId);