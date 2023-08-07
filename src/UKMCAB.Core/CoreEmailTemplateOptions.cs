namespace UKMCAB.Core;

public class CoreEmailTemplateOptions
{
    public string RegistrationRequest { get; set; } = null!;
    public string RegisterRequestConfirmation { get; set; } = null!;
    public string RegistrationApproved { get; set; } = null!;
    public string RegistrationRejected { get; set; } = null!;
        
    public string ResetPassword { get; set; } = null!;
    public string PasswordReset { get; set; } = null!;
    public string PasswordChanged { get; set; } = null!;

    public string FeedbackForm { get; set; } = null!;
    public string FeedbackEmail { get; set; } = null!;
    public string ContactUsUser { get; set; } = null!;
    public string ContactUsOPSS { get; set; } = null!;
    public string ContactUsOPSSEmail { get; set; } = null!;

    public string AccountRequestApproved { get; set; } = null!;
    public string AccountRequestRejected { get; set; } = null!;

    public string AccountLocked { get; set; } = null!;
    public string AccountUnlocked { get; set; } = null!;
    public string AccountArchived { get; set; } = null!;
    public string AccountUnarchived { get; set; } = null!;

}