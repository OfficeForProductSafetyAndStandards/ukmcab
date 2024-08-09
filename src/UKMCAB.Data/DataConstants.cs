namespace UKMCAB.Data;

public static class DataConstants
{
    public static class Version
    {
        public const string Number = "v4-2";
    }

    public static class CosmosDb
    {
        public const string Database = "main";
        public const string CabContainer = "cab-documents";
        public const string LegislativeAreasContainer = "legislative-areas";
    }

    public static class Search
    {
        public const int SearchResultsPerPage = 20;
        public const int SearchMaxPageRange = 3;
        public const int CABManagementQueueResultsPerPage = 10;
        public const string SEARCH_INDEX = "ukmcab-search-index-" + Version.Number ;
        public const string SEARCH_INDEXER = "ukmcab-search-indexer-" + Version.Number;
        public const string SEARCH_DATASOURCE = "ukmcab-search-datasource-" + Version.Number;
    }

    public static class Storage
    {
        public const string Container = "documents";
        public const string Documents = "documents";
        public const string Schedules = "schedules";
    }

    public static class SortOptions
    {
        public const string Default = "default";
        public const string LastUpdated = "lastupd";
        public const string A2ZSort = "a2z";
        public const string Z2ASort = "z2a";
    }
    public static class CabNumberVisibilityOptions
    {
        public const string Default = null;
        public const string Public = "public";
        public const string Internal = "internal";
        public const string Private = "private";
    }

    public static class Publications
    {
        public const string Public = "All users (public)";
        public const string Private = "Internal users";

    }
    public static class Lists
    {
        public static readonly List<string> Publications = new()
        {
            DataConstants.Publications.Public,
            DataConstants.Publications.Private,
        };

        public static readonly List<string> DocumentCategories = new()
        {
            "Appointment",
            "Recommendations",
            "Schedules",
            "Other"
        };

        public static readonly List<string> LegislativeAreas = new()
        {
            "Cableway installation",
            "Construction products",
            "Ecodesign",
            "Electromagnetic compatibility",
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

        public static readonly List<string> Countries = new()
        {
            "United Kingdom",
            "Afghanistan",
            "Albania",
            "Algeria",
            "Andorra",
            "Angola",
            "Antigua and Barbuda",
            "Argentina",
            "Armenia",
            "Australia",
            "Austria",
            "Azerbaijan",
            "Bahrain",
            "Bangladesh",
            "Barbados",
            "Belarus",
            "Belgium",
            "Belize",
            "Benin",
            "Bhutan",
            "Bolivia",
            "Bosnia and Herzegovina",
            "Botswana",
            "Brazil",
            "Brunei",
            "Bulgaria",
            "Burkina Faso",
            "Burundi",
            "Cambodia",
            "Cameroon",
            "Canada",
            "Cape Verde",
            "Central African Republic",
            "Chad",
            "Chile",
            "China",
            "Colombia",
            "Comoros",
            "Congo",
            "Congo (Democratic Republic)",
            "Costa Rica",
            "Croatia",
            "Cuba",
            "Cyprus",
            "Czechia",
            "Denmark",
            "Djibouti",
            "Dominica",
            "Dominican Republic",
            "East Timor",
            "Ecuador",
            "Egypt",
            "El Salvador",
            "Equatorial Guinea",
            "Eritrea",
            "Estonia",
            "Eswatini",
            "Ethiopia",
            "Fiji",
            "Finland",
            "France",
            "Gabon",
            "Georgia",
            "Germany",
            "Ghana",
            "Greece",
            "Grenada",
            "Guatemala",
            "Guinea",
            "Guinea-Bissau",
            "Guyana",
            "Haiti",
            "Honduras",
            "Hungary",
            "Iceland",
            "India",
            "Indonesia",
            "Iran",
            "Iraq",
            "Ireland",
            "Israel",
            "Italy",
            "Ivory Coast",
            "Jamaica",
            "Japan",
            "Jordan",
            "Kazakhstan",
            "Kenya",
            "Kiribati",
            "Kosovo",
            "Kuwait",
            "Kyrgyzstan",
            "Laos",
            "Latvia",
            "Lebanon",
            "Lesotho",
            "Liberia",
            "Libya",
            "Liechtenstein",
            "Lithuania",
            "Luxembourg",
            "Madagascar",
            "Malawi",
            "Malaysia",
            "Maldives",
            "Mali",
            "Malta",
            "Marshall Islands",
            "Mauritania",
            "Mauritius",
            "Mexico",
            "Micronesia",
            "Moldova",
            "Monaco",
            "Mongolia",
            "Montenegro",
            "Morocco",
            "Mozambique",
            "Myanmar (Burma)",
            "Namibia",
            "Nauru",
            "Nepal",
            "Netherlands",
            "New Zealand",
            "Nicaragua",
            "Niger",
            "Nigeria",
            "North Korea",
            "North Macedonia",
            "Norway",
            "Oman",
            "Pakistan",
            "Palau",
            "Panama",
            "Papua New Guinea",
            "Paraguay",
            "Peru",
            "Philippines",
            "Poland",
            "Portugal",
            "Qatar",
            "Romania",
            "Russia",
            "Rwanda",
            "Samoa",
            "San Marino",
            "Sao Tome and Principe",
            "Saudi Arabia",
            "Senegal",
            "Serbia",
            "Seychelles",
            "Sierra Leone",
            "Singapore",
            "Slovakia",
            "Slovenia",
            "Solomon Islands",
            "Somalia",
            "South Africa",
            "South Korea",
            "South Sudan",
            "Spain",
            "Sri Lanka",
            "St Kitts and Nevis",
            "St Lucia",
            "St Vincent",
            "Sudan",
            "Suriname",
            "Sweden",
            "Switzerland",
            "Syria",
            "Tajikistan",
            "Tanzania",
            "Thailand",
            "The Bahamas",
            "The Gambia",
            "Togo",
            "Tonga",
            "Trinidad and Tobago",
            "Tunisia",
            "Turkey",
            "Turkmenistan",
            "Tuvalu",
            "Uganda",
            "Ukraine",
            "United Arab Emirates",
            "United States",
            "Uruguay",
            "Uzbekistan",
            "Vanuatu",
            "Vatican City",
            "Venezuela",
            "Vietnam",
            "Yemen",
            "Zambia",
            "Zimbabwe"
        };

        public static readonly List<string> BodyTypes = new()
        {
            "Approved body",
            "NI Notified body",
            "Overseas body",
            "Recognised third-party",
            "Technical assessment body",
            "UK body designated under MRA: Australia",
            "UK body designated under MRA: Canada",
            "UK body designated under MRA: Japan",
            "UK body designated under MRA: New Zealand",
            "UK body designated under MRA: USA",
            "User inspectorate"
        };
    }

    public static class UserAccount
    {
        public const string UnassignedUserId = "unassigned-user-id";
    }    
    public static class PublishType
    {
        public const string MajorPublish = "Major publish";
        public const string MinorPublish = "Minor publish";
    }
}
