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
    [Description("Approve account request")]
    UserAccountRequest,
    [Description("CAB approved")]
    CABPublished,
    [Description("CAB declined")]
    CABDeclined,
    [Description("CAB published")]
    RequestToUnarchiveForPublishApproved,
    [Description("CAB unarchived")]
    RequestToUnarchiveForDraftApproved,
    [Description("Unarchive CAB request declined")]
    RequestToUnarchiveDeclined,
    UserAccountApproved,
    [Description("Draft CAB deleted")]
    DraftCabDeletedFromArchiving,
    [Description("Draft CAB deleted")]
    DraftCabDeleted,
    //Request to Unpublish / Archive
    [Description("Archive CAB request")]
    RequestToArchive,
    [Description("Unpublish CAB request")]
    RequestToUnpublish,
    [Description("Unpublish CAB request declined")]
    RequestToUnpublishDeclined,
    [Description("CAB unpublished")]
    RequestToUnpublishApproved,
    [Description("Approve legislative area request")]
    LegislativeAreaApproveRequestForCab,   
    [Description("Legislative area request declined")]
    LegislativeAreaDeclined,
    [Description("Remove legislative area request")]
    LegislativeAreaRequestToRemove,
    [Description("Archive legislative area request")]
    LegislativeAreaRequestToArchiveAndRemoveSchedule,
    [Description("Archive legislative area request")]
    LegislativeAreaRequestToArchiveAndArchiveSchedule,
    [Description("Remove legislative area request declined")]
    LegislativeAreaDeclinedToRemove,
    [Description("Archive legislative area request declined")]
    LegislativeAreaDeclinedToArchive,
    [Description("Unarchive legislative area request")]
    LegislativeAreaRequestToUnarchive,
    [Description("Unarchive legislative area request declined")]
    LegislativeAreaDeclinedToUnarchive,
    [Description("CAB review due")]
    CABReviewDue,
    [Description("Legislative area review due")]
    LAReviewDue,
}