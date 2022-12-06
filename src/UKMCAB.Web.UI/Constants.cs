namespace UKMCAB.Web.UI;

public static class Constants
{
    public const string MainLayoutPath = "~/Views/Shared/_Layout.cshtml";
    public const string SiteName = "UKMCAB alpha - GOV.UK";

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

    public static readonly List<string> LegislativeAreas = new()
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

