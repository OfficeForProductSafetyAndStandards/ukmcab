namespace UKMCAB.Web.UI.Models
{
    public class Constants
    {
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
}
