using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Text.RegularExpressions;
using UKMCAB.Common;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly SearchClient _indexClient;
        private const string LaDocumentsName = "DocumentLegislativeAreas/LegislativeAreaName";
        public SearchService(SearchClient searchClient)
        {
            _indexClient = searchClient;
        }

        public async Task<SearchFacets> GetFacetsAsync(bool internalSearch)
        {
            var provisionalLegislativeAreaPath = "DocumentLegislativeAreas/IsProvisional";
            var legislativeAreaStatus = "DocumentLegislativeAreas/Archived";
            var laStatus = "DocumentLegislativeAreas/Status";

            var result = new SearchFacets();
            var search = await _indexClient.SearchAsync<CABIndexItem>("*", new SearchOptions
            {
                Facets =
                {
                    $"{nameof(result.BodyTypes)},count:0", $"{LaDocumentsName},count:0",
                    $"{nameof(result.RegisteredOfficeLocation)},count:0", $"{nameof(result.StatusValue)},count:0",
                    $"{nameof(result.SubStatus)},count:0", $"{nameof(result.CreatedByUserGroup)},count:0",
                    $"{provisionalLegislativeAreaPath},count:0", $"{legislativeAreaStatus},count:0",
                    $"{laStatus},count:0"
                },
                Filter = internalSearch ? "" : "StatusValue eq '30' or StatusValue eq '40'"
            });

            if (search.HasValue)
            {
                var facets = search.Value.Facets;

                result.BodyTypes = GetFacetList(facets[nameof(result.BodyTypes)]);
                result.LegislativeAreas = GetFacetList(facets[LaDocumentsName]).ToList();
                result.RegisteredOfficeLocation = GetFacetList(facets[nameof(result.RegisteredOfficeLocation)]);
                result.StatusValue = GetFacetList(facets[nameof(result.StatusValue)]);
                result.CreatedByUserGroup = GetFacetList(facets[nameof(result.CreatedByUserGroup)]);
                result.SubStatus = GetFacetList(facets[nameof(result.SubStatus)]);
                result.ProvisionalLegislativeAreas =
                    GetFacetList(facets[provisionalLegislativeAreaPath]).OrderBy(x => x).ToList();
                result.LegislativeAreaStatus = GetLegislativeAreaStatusFacetList(facets[legislativeAreaStatus]).OrderBy(x => x).ToList();
                result.LAStatus = GetFacetList(facets[laStatus]);
            }

            return result;
        }

        public async Task<CABResults> QueryAsync(CABSearchOptions options)
        {
            var cabResults = new CABResults
            {
                PageNumber = options.PageNumber,
                Total = 0,
                CABs = new List<CABIndexItem>()
            };

            var query = GetKeywordsQuery(options.Keywords, options.InternalSearch);
            var filter = BuildFilter(options);
            var sort = BuildSort(options);

            var searchOptions = new SearchOptions
            {
                Size = options.IgnorePaging ? null : DataConstants.Search.SearchResultsPerPage,
                IncludeTotalCount = true,
                Skip = options.IgnorePaging
                    ? null
                    : DataConstants.Search.SearchResultsPerPage * (options.PageNumber - 1),
                Filter = filter,
                OrderBy = { sort },
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.Any,
            };

            if (options.Select.Count > 0)
            {
                options.Select.ForEach(x => searchOptions.Select.Add(x));
            }

            var search = await _indexClient.SearchAsync<CABIndexItem>(query, searchOptions);

            if (!search.HasValue)
            {
                return cabResults;
            }

            var results = search.Value.GetResults().ToList();
            if (!results.Any())
            {
                return cabResults;
            }

            var cabs = results.Select(r => r.Document).ToList();
            cabResults.CABs = cabs;
            cabResults.Total = Convert.ToInt32(search.Value.TotalCount);

            return cabResults;
        }

        public async Task ReIndexAsync(CABIndexItem cabIndexItem)
        {
            var response = await _indexClient.MergeOrUploadDocumentsAsync(new List<CABIndexItem> { cabIndexItem });
            Guard.IsTrue(response.HasValue && response.Value.Results.First().Succeeded,
                $"Failed to update index for {cabIndexItem.CABId}");
        }

        public async Task RemoveFromIndexAsync(string id)
        {
            await _indexClient.DeleteDocumentsAsync("id", new[] { id });
        }

        private string BuildFilter(CABSearchOptions options)
        {
            var filters = new List<string>();
            if (options.BodyTypesFilter != null && options.BodyTypesFilter.Any())
            {
                var bodyTypes = string.Join(" or ",
                    options.BodyTypesFilter.Select(bt => $"BodyTypes/any(bt: bt eq '{bt}')"));
                filters.Add($"({bodyTypes})");
            }

            if (options.LegislativeAreasFilter != null && options.LegislativeAreasFilter.Any())
            {
                string? legislativeAreas;
                if (options.InternalSearch == false)
                {
                    legislativeAreas = string.Join(" or ",
                        options.LegislativeAreasFilter.Select(la =>
                            $"DocumentLegislativeAreas/any(la: la/LegislativeAreaName eq '{la}' and la/Archived ne true)"));
                }
                else
                {
                    legislativeAreas = string.Join(" or ",
                        options.LegislativeAreasFilter.Select(la =>
                            $"DocumentLegislativeAreas/any(la: la/LegislativeAreaName eq '{la}')"));
                }

                filters.Add($"({legislativeAreas})");
            }

            if (options.RegisteredOfficeLocationsFilter != null && options.RegisteredOfficeLocationsFilter.Any())
            {
                var registeredOfficeLocations = string.Join(" or ",
                    options.RegisteredOfficeLocationsFilter.Select(rol => $"RegisteredOfficeLocation eq '{rol}'"));
                filters.Add($"({registeredOfficeLocations})");
            }

            if (options.StatusesFilter != null && options.StatusesFilter.Any())
            {
                var statuses = string.Join(" or ", options.StatusesFilter.Select(st => $"StatusValue eq '{st}'"));
                filters.Add($"({statuses})");
            }

            // Force the search to exlude all historical entries
            filters.Add($"StatusValue ne '{(int)Status.Historical}'");

            if (options.InternalSearch && options.UserGroupsFilter != null && options.UserGroupsFilter.Any())
            {
                var userGroups = string.Join(" or ",
                    options.UserGroupsFilter.Select(ug => $"CreatedByUserGroup eq '{ug}'"));
                filters.Add($"({userGroups})");
            }

            if (options.InternalSearch && options.SubStatusesFilter != null && options.SubStatusesFilter.Any())
            {
                var subStatuses = string.Join(" or ", options.SubStatusesFilter.Select(st => $"SubStatus eq '{st}'"));
                filters.Add($"({subStatuses})");
            }

            if (options.InternalSearch && options.ProvisionalLegislativeAreasFilter != null &&
                options.ProvisionalLegislativeAreasFilter.Any())
            {
                if (options.ProvisionalLegislativeAreasFilter.Length == 1)
                {
                    if (options.ProvisionalLegislativeAreasFilter.Contains("True"))
                    {
                        filters.Add($"DocumentLegislativeAreas/any(la: la/IsProvisional eq true)");
                    }
                    else
                    {
                        filters.Add($"DocumentLegislativeAreas/all(la: la/IsProvisional ne true)");
                    }
                }
            }
            if (options is { InternalSearch: true, LegislativeAreaStatusFilter: not null } &&
                options.LegislativeAreaStatusFilter.Any())
            {
                 if (options.LegislativeAreaStatusFilter.Count() == 1)
                {
                    if (options.LegislativeAreaStatusFilter.Contains("True"))
                    {
                        filters.Add($"DocumentLegislativeAreas/any(la: la/Archived eq true)");
                    }
                    else
                    {
                        filters.Add($"DocumentLegislativeAreas/all(la: la/Archived ne true) and DocumentLegislativeAreas/any()");
                    }
                }
            }

            if (options is { InternalSearch: true, LAStatusFilter: not null } &&
                options.LAStatusFilter.Any())
            {
                var lastatuses = string.Join(" or ", options.LAStatusFilter.Select(st => $"DocumentLegislativeAreas/any(la: la/Status eq '{st}')"));
                filters.Add($"({lastatuses})");
            }
            // if internal search (user logged in) and non opss user (ukas user) then exclude opss draft cab from search

            if (options.InternalSearch && !options.IsOPSSUser)
            {
                filters.Add($"not (CreatedByUserGroup eq 'opss' and StatusValue eq '{(int)Status.Draft}')");
            }

            return filters.Count > 1 ? $"({string.Join(" and ", filters)})" : filters.FirstOrDefault() ?? string.Empty;
        }

        private string BuildSort(CABSearchOptions options)
        {
            switch (options.Sort)
            {
                case DataConstants.SortOptions.LastUpdated:
                    return "LastUpdatedDate desc";

                case DataConstants.SortOptions.A2ZSort:
                    return "Name asc";

                case DataConstants.SortOptions.Z2ASort:
                    return "Name desc";

                case DataConstants.SortOptions.Default:
                default:
                    return string.IsNullOrWhiteSpace(options.Keywords) ? "RandomSort asc" : string.Empty;
            }
        }

        private List<string> GetFacetList(IList<FacetResult> facets)
        {
            var list = facets.Select(f => f.Value.ToString()).OrderBy(f => f).ToList();
            if (list.Contains("United Kingdom"))
            {
                list.Remove("United Kingdom");
                list.Insert(0, "United Kingdom");
            }

            return list;
        }
        private IEnumerable<string> GetLegislativeAreaStatusFacetList(IEnumerable<FacetResult> facets)
            => facets.Select(f => f.Value.ToString()).OrderBy(f => f).ToList();
        
        private static readonly Regex SpecialCharsRegex = new("[+&|\\[!()\\]{}\\^\"~*?:\\/]");

        private string GetKeywordsQuery(string? keywords, bool internalSearch)
        {
            string retVal;
            var input = (keywords ?? string.Empty).Trim();
            if (input.Contains("~lucene"))
            {
                retVal = input.Replace("~lucene", string.Empty).Trim();
            }
            else
            {
                input = SpecialCharsRegex.Replace(input, " ");
                if (input.Clean() == null)
                {
                    retVal = "*";
                }
                else
                {
                    input = SpecialCharsRegex.Replace(input, " ");
                    var tokens = new List<string>
                    {
                        $"{nameof(CABIndexItem.Name)}:({input})^3", //any-match, boosted x3
                        $"{nameof(CABIndexItem.TownCity)}:({input})", //any-match
                        $"{nameof(CABIndexItem.Postcode)}:(\"{input}\")", //phrase-match
                        $"{nameof(CABIndexItem.HiddenText)}:(\"{input}\")", //phrase-match
                        //$"{nameof(CABIndexItem.ScheduleLabels)}:(\"{input}\")",        // TODO: removed from 2.0 phrase-match
                        $"{nameof(CABIndexItem.CABNumber)}:(\"{input}\")^4", //phrase-match, boosted x4
                        $"{nameof(CABIndexItem.PreviousCABNumbers)}:(\"{input}\")^4", //phrase-match, boosted x4
                         $"{LaDocumentsName}:(\"{input}\")^6", //phrase-match, boosted x6
                        $"{nameof(CABIndexItem.HiddenScopeOfAppointments)}:(\"{input}\")^6", //phrase-match, boosted x6
                        $"{nameof(CABIndexItem.UKASReference)}:(\"{input}\")", //phrase-match
                    };
                    if (internalSearch)
                    {
                        tokens.Add($"{nameof(CABIndexItem.DocumentLabels)}:(\"{input}\")"); //phrase-match
                    }

                    retVal = string.Join(" ", tokens);
                }
            }

            return retVal;
        }
    }
}