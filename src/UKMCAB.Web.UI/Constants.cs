namespace UKMCAB.Web.UI;

public static class Constants
{
    public const string MainLayoutPath = "~/Views/Shared/_Layout.cshtml";
    public const string SiteName = "UKMCAB alpha - GOV.UK";

    public class UriPaths
    {
        private const string _errorFolder = "/jk41df7d191/";
        public const string Error404 = _errorFolder + "404";
        public const string Error400DomainException = _errorFolder + "400";
        public const string Error403Forbidden = _errorFolder + "403";
        public const string Error500UnhandledException = _errorFolder + "500";
        public const string LoggedOff = "/LoggedOff";
    }
}

