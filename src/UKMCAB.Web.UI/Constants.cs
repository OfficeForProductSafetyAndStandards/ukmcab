using System.Security.Policy;

namespace UKMCAB.Web.UI;

public static class Constants
{
    public const string MainLayoutPath = "~/Views/Shared/_Layout.cshtml";
    public const string SiteName = "UKMCAB";
    public const string NotProvided = "Not provided";

    public const string TempDraftKey = nameof(TempDraftKey);
    public const string TempCookieChangeKey = nameof(TempCookieChangeKey);

    public const string AnalyticsOptInCookieName = "accept_analytics_cookies";

    public class PageTitle
    {
        public const string About = "About";
        public const string AccessibilityStatement = "Accessibility Statement";
        public const string CookiesPolicy = "Cookies Policy";
        public const string Help = "Help";
        public const string PrivacyNotice = "Privacy Notice";
        public const string TermsAndConditions = "Terms And Conditions";
        public const string Updates = "Updates";
    }
    public static class Config
    {
        public const string ContainerNameDataProtectionKeys = "dataprotectionkeys";
    }

    public class SubmitType
    {
        public const string Continue = nameof(Continue);
        public const string Save = nameof(Save);
    }

    public class Roles
    {
        public const string OPSSAdmin = nameof(OPSSAdmin);
        public const string UKASUser = nameof(UKASUser);
        public const string OGDUser = nameof(OGDUser);

        public static readonly List<string> AuthRoles = new List<string>
        {
            nameof(OPSSAdmin),
            nameof(UKASUser),
            nameof(OGDUser),
        };
    }

    public class ErrorMessages
    {
        public const string InvalidLoginAttempt = "The information provided is not right, try again";
    }
}

public class Nonces
{
    public const string GoogleAnalyticsScript = "2490d105cb874";
    public const string GoogleAnalyticsInlineScript = "b1b40840d8d84";
    public const string AppInsights = "VQ8uRGcAff";
    public const string Main = "uKK1n1fxoi";
}

