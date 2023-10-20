using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.Models.Task;

public abstract record Task(Guid Id, TaskType TaskType, TaskState State, UserAccount Submitter, UserAccount Assignee, DateTime? Assigned,
    string Reason, DateTime SentOn, UserAccount LastUpdatedBy, DateTime LastUpdatedOn, bool? Approved,
    string? DeclineReason, bool Completed, Guid documentId);