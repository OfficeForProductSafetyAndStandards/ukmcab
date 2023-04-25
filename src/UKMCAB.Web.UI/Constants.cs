using System.Security.Policy;

namespace UKMCAB.Web.UI;

public static class Constants
{
    public const string MainLayoutPath = "~/Views/Shared/_Layout.cshtml";
    public const string SiteName = "UKMCAB alpha - GOV.UK";
    public const string NotProvided = "Not provided";

    public const string TempDraftKey = nameof(TempDraftKey);

    public class SubmitType
    {
        public const string Continue = nameof(Continue);
        public const string Save = nameof(Save);
    }

    public static class Config
    {
        public const string ContainerNameDataProtectionKeys = "dataprotectionkeys";
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

