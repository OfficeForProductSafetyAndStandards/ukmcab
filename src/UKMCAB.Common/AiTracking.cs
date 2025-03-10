﻿namespace UKMCAB.Common;

public static class AiTracking
{
    public static class Events
    {
        public const string LoginSuccess = "app_login_success";
        public const string LoginFailed = "app_login_failed";
        public const string Logout = "app_login";
        public const string Lockout = "app_lockout";
        public const string ChangedPassword = "app_changed_password";
        public const string UserEditedProfile = "app_user_edited_profile";

        public const string InternalUserCreated = "app_created_internal_user";

        public const string CabCreated = "app_cab_created";
        public const string CabUpdated = "app_cab_updated";
        public const string CabArchived = "app_cab_archived";
        public const string CabUnpublished = "app_cab_unpublished";
        public const string CabUnarchived = "app_cab_unarchived";
        public const string CabViewed = "app_cab_viewed";
        public const string CabViewedViaSubscriptionsEmail = "app_cab_viewed_subs_inbound";
        public const string CabsSearched = "app_cab_searched";

        public const string UsersCount = "app_users_count";

        public const string SubscriptionsInitialise = "app_subscriptions_initialise";
        public const string RandomSortGeneratorRun = "app_randomsortgenerator_run";
    }

    public static class Metrics
    {
        public const string CabsByStatusFormat = "app_cabs_count_by_status_{0}";
        public const string CabSubscriptionsCount = "app_subscriptions_cab_count";
        public const string SearchSubscriptionsCount = "app_subscriptions_search_count";
        public const string CabsWithSchedules = "app_cabs_with_schedules";
        public const string CabsWithoutSchedules = "app_cabs_without_schedules";
        public const string RandomSortGeneratorRunDateTime = "app_randomsortgenerator_run_datetime";
    }

    public static class Metadata
    {
        public const string UserAgent = "user_agent";
        public const string User = "user";
        public const string CabId = "cab";
        public const string CabName = "cab_name";
        public const string ResultsCount = "results_count";
        public const string WithCriteria = "with_criteria";
    }
}