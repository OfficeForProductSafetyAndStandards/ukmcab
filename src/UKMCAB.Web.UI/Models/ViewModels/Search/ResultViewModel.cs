using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ResultViewModel
    {
        public ResultViewModel(CABIndexItem cab)
        {
            Name = cab.Name;
            URLSlug = cab.URLSlug;
            Address = StringExt.Join(", ", cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country);
            BodyType = cab.BodyTypes.Sentenceify();
            RegisteredOfficeLocation = cab.RegisteredOfficeLocation;
            RegisteredTestLocation = cab.TestingLocations.Sentenceify();
            LegislativeArea = cab.LegislativeAreas.Sentenceify();
        }

        public string Name { get; set; }
        public string URLSlug { get; set; }
        public string Address { get; set; }
        public string BodyType { get; set; }
        public string RegisteredOfficeLocation { get; set; }
        public string RegisteredTestLocation { get; set; }
        public string LegislativeArea { get; set; }
    }
}
