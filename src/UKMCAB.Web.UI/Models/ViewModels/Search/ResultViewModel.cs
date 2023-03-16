using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ResultViewModel
    {
        public ResultViewModel(CABIndexItem cab)
        {
            id = cab.id;
            Name = cab.Name;
            Address = cab.Address;
            BodyType = ListToString(cab.BodyTypes);
            RegisteredOfficeLocation = cab.RegisteredOfficeLocation;
            RegisteredTestLocation = ListToString(cab.TestingLocations);
            LegislativeArea = ListToString(cab.LegislativeAreas);
        }

        public string id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string BodyType { get; set; }
        public string RegisteredOfficeLocation { get; set; }
        public string RegisteredTestLocation { get; set; }
        public string LegislativeArea { get; set; }

        private string ListToString(string[] list)
        {
            if (list == null || list.Length == 0)
            {
                return string.Empty;
            }

            if (list.Length == 1)
            {
                return list.First();
            }

            return $"{list.First()} and {list.Length - 1} other{(list.Length > 2 ? "s" : string.Empty)}";
        }
    }
}
