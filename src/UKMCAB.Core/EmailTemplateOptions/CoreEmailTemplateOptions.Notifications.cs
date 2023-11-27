namespace UKMCAB.Core.EmailTemplateOptions;

public partial class CoreEmailTemplateOptions
{
    public string NotificationRequestToPublish { get; set; } = null!;
    public string NotificationRequestToPublishEmail { get; set; } = null!;
    public string NotificationCabApproved { get; set; } = null!;
    public string NotificationCabDeclined { get; set; } = null!;
}