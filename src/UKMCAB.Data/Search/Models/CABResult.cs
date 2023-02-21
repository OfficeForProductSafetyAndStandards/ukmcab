using System.Security.AccessControl;

namespace UKMCAB.Data.Search.Models
{
    public class CABResult
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<string> BodyTypes { get; set; }
        public string RegisteredOfficeLocation { get; set; }
        public List<string> LegislativeAreas { get; set; }
    }
}
