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
        public List<FilterOption> BodyTypeOptions { get; set; }
        public List<FilterOption> RegisteredOfficeLocationOptions { get; set; }
        public List<FilterOption> TestingLocationOptions { get; set; }
        public List<FilterOption> LegislativeAreaOptions { get; set; }

        public int FilterCount => (BodyTypes?.Length ?? 0) + (RegisteredOfficeLocations?.Length ?? 0) + (TestingLocations?.Length ?? 0) + (LegislativeAreas?.Length ?? 0);

        // Results
        public List<ResultViewModel> SearchResults { get; set; }
        public PaginationViewModel Pagination { get; set; }
    }
}
