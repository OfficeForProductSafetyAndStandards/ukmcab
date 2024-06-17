using UKMCAB.Common.Extensions;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class SearchViewModel : ILayoutModel
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
        public string? FilterPath { get; set; }
        public bool InternalSearch { get; set; }
        public bool IsOPSSUser { get; set; }

        // ILayout
        public virtual string? Title => Keywords.IsNotNullOrEmpty() ? Keywords : "Search";


        // Form fields
        public string? Keywords { get; set; }
        public string[]? BodyTypes { get; set; }
        public string[]? RegisteredOfficeLocations { get; set; }
        public string[]? LegislativeAreas { get; set; }
        public string[]? Statuses { get; set; }
        public string[]? UserGroups { get; set; }
        public string[]? SubStatuses { get; set; }
        public string[]? ProvisionalLegislativeAreas { get; set; }
        public string[]? ArchivedLegislativeArea { get; set; }
        public string[]? LAStatus { get; set; }
        public string? Sort { get; set; }
        public int PageNumber { get; set; } = 1;
        public bool SelectAllPendingApproval { get; set; }

        // Form elements
        public FilterViewModel? BodyTypeOptions { get; set; }
        public FilterViewModel? RegisteredOfficeLocationOptions { get; set; }
        public FilterViewModel? LegislativeAreaOptions { get; set; }
        public FilterViewModel? StatusOptions { get; set; }
        public FilterViewModel? CreatedByUserGroupOptions { get; set; }
        public FilterViewModel? SubStatusOptions { get; set; }
        public FilterViewModel? LegislativeAreaProvisionalOptions { get; set; }

        public FilterViewModel? LegislativeAreaStatusOptions { get; set; }
        public FilterViewModel? LAStatusOptions { get; set; }

        public int FilterCount => (BodyTypes?.Length ?? 0) + (RegisteredOfficeLocations?.Length ?? 0) +
                                  (LegislativeAreas?.Length ?? 0) +
                                  (Statuses != null && (InternalSearch ||
                                                        Statuses.Contains(((int)Status.Archived).ToString()))
                                      ? Statuses.Length
                                      : 0) +
                                  (SubStatuses != null && InternalSearch ? SubStatuses.Length : 0) +
                                  (UserGroups != null && InternalSearch ? UserGroups.Length : 0) +
                                  (ProvisionalLegislativeAreas != null && InternalSearch
                                      ? ProvisionalLegislativeAreas.Length
                                      : 0) +
                                  (ArchivedLegislativeArea != null && InternalSearch ? ArchivedLegislativeArea.Length : 0) +
                                  (LAStatus != null && InternalSearch ? LAStatus.Length : 0);                                  

        public Dictionary<string, string[]> SelectedFilters => new()
        {
            { nameof(BodyTypes), BodyTypes ?? Array.Empty<string>() },
            { nameof(RegisteredOfficeLocations), RegisteredOfficeLocations ?? Array.Empty<string>() },
            { nameof(LegislativeAreas), LegislativeAreas ?? Array.Empty<string>() },
            { nameof(Statuses), Statuses ?? Array.Empty<string>() },
            { nameof(UserGroups), UserGroups ?? Array.Empty<string>() },
            { nameof(SubStatuses), SubStatuses ?? Array.Empty<string>() },
            { nameof(ProvisionalLegislativeAreas), ProvisionalLegislativeAreas ?? Array.Empty<string>() },
            { nameof(ArchivedLegislativeArea), ArchivedLegislativeArea ?? Array.Empty<string>()},
            { nameof(LAStatus), LAStatus ?? Array.Empty<string>()}
        };

        public string StatusLabel(string status)
        {
            return $"{(Status)int.Parse(status)} CAB";
        }

        public string SubStatusLabel(string substatus)
        {
            return ((SubStatus)int.Parse(substatus)).GetEnumDescription();
        }

        public string UserGroupLabel(string userGroup)
        {
            return userGroup.ToUpper();
        }

        public string ProvisionalLegislativeAreaLabel(string value)
        {
            return bool.Parse(value) ? "Provisional legislative area: Yes" : "Provisional legislative area: No";
        }
        public string LegislativeAreaStatusLabel(string value)
        {
            return  bool.Parse(value) ? "Archived legislative area: Yes" : "Archived legislative area: No";
        }
        public string LAStatusLabel(string status)
        {
            return ((LAStatus)int.Parse(status)).GetEnumDescription();
        }
        public List<SortOption> SortOptions => new List<SortOption>
        {
            new SortOption
            {
                Label = string.IsNullOrWhiteSpace(Keywords) ? "Random" : "Relevant",
                Value = DataConstants.SortOptions.Default,
                AriaSort = "other"
            },
            new SortOption
            {
                Label = "Last updated",
                Value = DataConstants.SortOptions.LastUpdated,
                AriaSort = "descending"
            },
            new SortOption
            {
                Label = "A to Z",
                Value = DataConstants.SortOptions.A2ZSort,
                AriaSort = "ascending"
            },
            new SortOption
            {
                Label = "Z to A",
                Value = DataConstants.SortOptions.Z2ASort,
                AriaSort = "descending"
            }
        };


        // Results
        public List<ResultViewModel>? SearchResults { get; set; }
        public PaginationViewModel? Pagination { get; set; }
        public FeedLinksViewModel? FeedLinksViewModel { get; set; }
    }

    public class SortOption
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public string? AriaSort { get; set; }
    }
}