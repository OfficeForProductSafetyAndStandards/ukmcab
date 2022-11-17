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

    public class Roles
    {
        public const string Administrator = nameof(Administrator);
        public const string UKASUser = nameof(UKASUser);
        public const string OGDUser = nameof(OGDUser);
    }

    public static readonly List<string> LegislativeAreas = new List<string>
    {
        "Cableway Installation",
        "Construction Products",
        "Ecodesign",
        "Electromagnetic Compatibility",
        "Equipment and protective systems for use in potentially explosive atmospheres",
        "Explosives",
        "Gas appliances and related",
        "Lifts",
        "Machinery",
        "Marine equipment",
        "Measuring instruments",
        "Medical devices",
        "Noise emissions in the environment by equipment for use outdoors",
        "Non-automatic weighing instruments",
        "Personal protective equipment",
        "Pressure equipment",
        "Pyrotechnics",
        "Radio equipment",
        "Railway interoperability",
        "Recreational craft",
        "Simple pressure vessels",
        "Toys",
        "Transportable pressure equipment"
    };
}

