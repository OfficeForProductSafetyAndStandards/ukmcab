namespace UKMCAB.Web.UI;

public static class Constants
{
    public const string MainLayoutPath = "~/Views/Shared/_Layout.cshtml";
    public const string SiteName = "UK Market Conformity Assessment Bodies";
    public const string NotProvided = "Not provided";
    public const string NotApplicable = "Not applicable";
    public const string None = "None";
    public const string NotAssigned = "Not assigned";
    public const string Select = "Select";
    public const string DeclinedLA = nameof(DeclinedLA);
    public const string ApprovedLA = nameof(ApprovedLA);

    public const string TempDraftKeyLine1 = nameof(TempDraftKeyLine1);
    public const string TempDraftKeyLine2 = nameof(TempDraftKeyLine2);
    public const string TempCookieChangeKey = nameof(TempCookieChangeKey);

    public const string AnalyticsOptInCookieName = "accept_analytics_cookies";

    public const string DateTimeFormat = "dd/MM/yyyy HH:mm";
    public const int RowsPerPage = 10;

    public class PageTitle
    {
        public const string About = "About";
        public const string AccessibilityStatement = "Accessibility Statement";
        public const string CookiesPolicy = "Cookies Policy";
        public const string Help = "Help";
        public const string PrivacyNotice = "Privacy Notice";
        public const string TermsAndConditions = "Terms And Conditions";
        public const string Updates = "Updates";
        public const string Notifications = "Notifications for";
        public const string Home = "Home";
    }

    public class Heading
    {
        public const string CabDetails = "CAB details";
       
    }
    public static class Config
    {
        public const string ContainerNameDataProtectionKeys = "dataprotectionkeys";
    }

    public class SubmitType
    {
        public const string Continue = nameof(Continue);
        public const string SubmitForApproval = nameof(SubmitForApproval);
        public const string Save = nameof(Save); //Save as draft
        public const string Add18 = nameof(Add18);
        public const string UploadAnother = nameof(UploadAnother);
        public const string UseFileAgain = nameof(UseFileAgain);
        public const string ReplaceFile = nameof(ReplaceFile);
        public const string Approve = nameof(Approve);
        public const string Reject = nameof(Reject);
        public const string Remove = nameof(Remove);
        public const string Cancel = nameof(Cancel);
        public const string AdditionalInfo = nameof(AdditionalInfo);
        public const string Confirm = nameof(Confirm);
        public const string Add = nameof(Add);
        public const string Edit = nameof(Edit);
        public const string Archive = nameof(Archive);
        public const string RemoveArchived = nameof(RemoveArchived);
        public const string Search = nameof(Search);
        public const string PaginatedQuery = nameof(PaginatedQuery);
        public const string ShowAllSelected = nameof(ShowAllSelected);
        public const string ClearShowAllSelected = nameof(ClearShowAllSelected);
        public const string MRABypass = nameof(MRABypass);
    }

    public class ErrorMessages
    {
        public const string InvalidLoginAttempt = "Enter a valid email address and password";
        public const string DuplicateEntry = "Duplicate scope of appointment - the previous entry could not be saved.";
    }
}

public class Nonces
{
    public const string GoogleAnalyticsScript = "2490d105cb874";
    public const string GoogleAnalyticsInlineScript = "b1b40840d8d84";
    public const string AppInsights = "VQ8uRGcAff";
    public const string Main = "uKK1n1fxoi";
}

