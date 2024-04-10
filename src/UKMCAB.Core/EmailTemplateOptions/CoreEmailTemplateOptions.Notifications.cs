namespace UKMCAB.Core.EmailTemplateOptions;

public partial class CoreEmailTemplateOptions
{
    public string NotificationRequestToPublish { get; set; } = null!;
    public string ApprovedBodiesEmail { get; set; } = null!;
    public string NotificationCabApproved { get; set; } = null!;
    public string NotificationLegislativeAreaCabApproval { get; set; } = null!;
    public string NotificationLegislativeAreaApproved { get; set; } = null!;
    public string NotificationLegislativeAreaDeclined { get; set; } = null!;
    public string NotificationLegislativeAreaRequestToRemove { get; set; } = null!;
    public string NotificationLegislativeAreaRequestToRemoveApproved { get; set; } = null!;
    public string NotificationLegislativeAreaRequestToRemoveDeclined { get; set; } = null!;
    public string NotificationCabDeclined { get; set; } = null!;
    public string NotificationUnarchiveForApproval { get; set; } = null!;
    public string NotificationDraftCabDeleted { get; set; } = null!;
    public string NotificationDraftCabDeletedFromArchiving { get; set; } = null!;
    public string NotificationUnarchiveApproved { get; set; } = null!;
    public string NotificationUnarchiveDeclined { get; set; } = null!;
    public string NotificationRequestToUnpublishCab { get; set; } = null!;
    public string NotificationUnpublishDeclined { get; set; } = null!;
    public string NotificationUnpublishApproved { get; set; } = null!;
}