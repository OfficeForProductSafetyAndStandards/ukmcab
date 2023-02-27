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
        public string Sort { get; set; }
        public int PageNumber { get; set; }

        // Form elements
        public FilterViewModel BodyTypeOptions { get; set; }
        public FilterViewModel RegisteredOfficeLocationOptions { get; set; }
        public FilterViewModel TestingLocationOptions { get; set; }
        public FilterViewModel LegislativeAreaOptions { get; set; }

        public int FilterCount => (BodyTypes?.Length ?? 0) + (RegisteredOfficeLocations?.Length ?? 0) + (TestingLocations?.Length ?? 0) + (LegislativeAreas?.Length ?? 0);
        public Dictionary<string, string> SortOptions => new()
        {
            { string.IsNullOrWhiteSpace(Keywords) ? "Random" : "Relevant" , ""},
            {"Last updated", Constants.LastUpdatedSortValue},
            {"A to Z", Constants.A2ZSortValue},
            {"Z to A", Constants.Z2ASortValue}
        };

        // Results
        public List<ResultViewModel> SearchResults { get; set; }
        public PaginationViewModel Pagination { get; set; }
    }
}
