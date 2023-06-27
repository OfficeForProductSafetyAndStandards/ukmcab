using UKMCAB.Data;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class SearchViewModel: ILayoutModel
    {
        /// <summary>
        ///     Defines the list of properties that are not filters; such as paging or sorting info
        /// </summary>
        /// <remarks>
        ///     This is used by Subscriptions Core so that it can retrieve search results that are not paged or sorted
        /// </remarks>
        public static readonly string[] NonFilterProperties = new[] { nameof(Sort), nameof(PageNumber) };

        /// <summary>
        /// Gets the name of the property that contains the query string item for 'keywords'
        /// </summary>
        /// <remarks>
        /// NOTE: This is used by the email subscriptions functionality 
        /// </remarks>
        public static string GetKeywordsQueryStringKey() => nameof(Keywords);

        public string? ReturnUrl { get; set; }

        // ILayout
        public string? Title => Keywords.IsNotNullOrEmpty()? Keywords: "Search";

        // Form fields
        public string Keywords { get; set; }
        public string[] BodyTypes { get; set; }
        public string[] RegisteredOfficeLocations { get; set; }
        public string[] LegislativeAreas { get; set; }
        public string Sort { get; set; } = DataConstants.SortOptions.Default;
        public int PageNumber { get; set; } = 1;

        // Form elements
        public FilterViewModel BodyTypeOptions { get; set; }
        public FilterViewModel RegisteredOfficeLocationOptions { get; set; }
        public FilterViewModel LegislativeAreaOptions { get; set; }

        public int FilterCount => (BodyTypes?.Length ?? 0) + (RegisteredOfficeLocations?.Length ?? 0) + (LegislativeAreas?.Length ?? 0);
        public Dictionary<string, string> SortOptions => new()
        {
            { string.IsNullOrWhiteSpace(Keywords) ? "Random" : "Relevant" , DataConstants.SortOptions.Default},
            {"Last updated", DataConstants.SortOptions.LastUpdated},
            {"A to Z", DataConstants.SortOptions.A2ZSort},
            {"Z to A", DataConstants.SortOptions.Z2ASort}
        };
            
        // Results
        public List<ResultViewModel> SearchResults { get; set; }
        public PaginationViewModel Pagination { get; set; }
        public FeedLinksViewModel FeedLinksViewModel { get; set; }
    }
}
