namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class SearchViewModel: ILayoutModel
    {
        // ILayout
        public string? Title => "Search";

        // Form fields
        public string Keywords { get; set; }
        public string[] BodyTypes { get; set; }
        public string[] RegisteredOfficeLocations { get; set; }
        public string[] TestingLocations { get; set; }
        public string[] LegislativeAreas { get; set; }

        public int PageNumber { get; set; }

        // Form elements
        public List<string> BodyTypeOptions { get; set; }
        public List<string> RegisteredOfficeLocationOptions { get; set; }
        public List<string> TestingLocationOptions { get; set; }
        public List<string> LegislativeAreaOptions { get; set; }

        // Results
        public List<ResultViewModel> SearchResults { get; set; }
        public PaginationViewModel Pagination { get; set; }
    }
}
