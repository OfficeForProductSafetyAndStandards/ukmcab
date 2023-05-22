namespace UKMCAB.Common;

public static class AiTracking
{
    public static class Events
    {
        public const string LoginSuccess = "app_login_success";
        public const string LoginFailed = "app_login_failed";
        public const string Logout = "app_login";
        public const string Lockout = "app_lockout";
        public const string ChangedPassword = "app_changed_password";

        public const string CabCreated = "app_cab_created";
        public const string CabUpdated = "app_cab_updated";
        public const string CabArchived = "app_cab_archived";
        public const string CabViewed = "app_cab_viewed";
    }

    public static class Metadata
    {
        public const string UserAgent = "user_agent";
        public const string User = "user";
        public const string CabId = "cab";
        public const string CabName = "cab_name";
    }
}