using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UKMCAB.Common;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private static readonly Regex _specialCharsRegex = new("[+&|\\[!()\\]{}\\^\"~*?:\\/]");
        private readonly SearchIndexerClient _searchIndexerClient;
        private readonly TelemetryClient _telemetryClient;
        private SearchClient _indexClient;
        public SearchService(SearchClient searchClient, SearchIndexerClient searchIndexerClient, TelemetryClient telemetryClient)
        {
            _indexClient = searchClient;
            _searchIndexerClient = searchIndexerClient;
            _telemetryClient = telemetryClient;
        }

        public async Task<SearchFacets> GetFacetsAsync()
        {
            var result = new SearchFacets();
            var search = await _indexClient.SearchAsync<CABIndexItem>("*", new SearchOptions
            {
                Facets = { $"{nameof(result.BodyTypes)},count:0", $"{nameof(result.LegislativeAreas)},count:0", $"{nameof(result.RegisteredOfficeLocation)},count:0" }
            });
            if (search.HasValue)
            {
                var facets = search.Value.Facets;

                result.BodyTypes = GetFacetList(facets[nameof(result.BodyTypes)]);
                result.LegislativeAreas = GetFacetList(facets[nameof(result.LegislativeAreas)]).Select(x => x.ToSentenceCase()).ToList()!;
                result.RegisteredOfficeLocation = GetFacetList(facets[nameof(result.RegisteredOfficeLocation)]);
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

            var query = GetKeywordsQuery(options.Keywords);
            var filter = BuildFilter(options);
            var sort = BuildSort(options);

            var searchOptions = new SearchOptions
            {
                Size = options.IgnorePaging ? null : DataConstants.Search.SearchResultsPerPage,
                IncludeTotalCount = true,
                Skip = options.IgnorePaging ? null : DataConstants.Search.SearchResultsPerPage * (options.PageNumber - 1),
                Filter = filter,
                OrderBy = { sort },
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.Any,
            };

            searchOptions.HighlightFields.Add(nameof(CABIndexItem.Name));
            searchOptions.HighlightFields.Add(nameof(CABIndexItem.TownCity));
            searchOptions.HighlightFields.Add(nameof(CABIndexItem.Postcode));
            searchOptions.HighlightFields.Add(nameof(CABIndexItem.HiddenText));
            searchOptions.HighlightFields.Add(nameof(CABIndexItem.CABNumber));
            searchOptions.HighlightFields.Add(nameof(CABIndexItem.LegislativeAreas));

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


        public async Task ReIndexAsync(Document doc)
        {
            var response = await _indexClient.MergeOrUploadDocumentsAsync<CABIndexItem>(new List<CABIndexItem> {new CABIndexItem
            {
                id = doc.id,
                Name = doc.Name,
                CABId = doc.CABId,
                CABNumber = doc.CABNumber,
                AddressLine1 = doc.AddressLine1,
                AddressLine2 = doc.AddressLine1,
                TownCity = doc.TownCity,
                County = doc.County,
                Postcode = doc.Postcode,
                Country = doc.Country,
                Email = doc.Email,
                Phone = doc.Phone,
                Website = doc.Website,
                BodyTypes = doc.BodyTypes.ToArray(),
                TestingLocations = doc.TestingLocations.ToArray(),
                LegislativeAreas = doc.LegislativeAreas.ToArray(),
                RegisteredOfficeLocation = doc.RegisteredOfficeLocation,
                URLSlug = doc.URLSlug,
                LastUpdatedDate = doc.LastUpdatedDate,
                RandomSort = doc.RandomSort,
            }} );

            Guard.IsTrue(response.HasValue && response.Value.Results.First().Succeeded, $"Failed to update index for {doc.CABId}");
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
                var bodyTypes = string.Join(" or ", options.BodyTypesFilter.Select(bt => $"BodyTypes/any(bt: bt eq '{bt}')"));
                filters.Add($"({bodyTypes})");
            }
            if (options.LegislativeAreasFilter != null && options.LegislativeAreasFilter.Any())
            {
                var legislativeAreas = string.Join(" or ", options.LegislativeAreasFilter.Select(bt => $"LegislativeAreas/any(la: la eq '{bt}')"));
                filters.Add($"({legislativeAreas})");
            }
            if (options.RegisteredOfficeLocationsFilter != null && options.RegisteredOfficeLocationsFilter.Any())
            {
                var registeredOfficeLocations = string.Join(" or ", options.RegisteredOfficeLocationsFilter.Select(bt => $"RegisteredOfficeLocation eq '{bt}'"));
                filters.Add($"({registeredOfficeLocations})");
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

        private string GetKeywordsQuery(string? keywords)
        {
            string retVal;
            var input = (keywords ?? string.Empty).Trim();
            if (input.Contains("~lucene"))
            {
                retVal = input.Replace("~lucene", string.Empty).Trim();
            }
            else
            {
                if (input.Clean() == null)
                {
                    retVal = "*";
                }
                else
                {
                    var tokens = new[]
                    {
                        $"{nameof(CABIndexItem.Name)}:({keywords})^3",                    //any-match, boosted x3
                        $"{nameof(CABIndexItem.TownCity)}:({keywords})",                  //any-match
                        $"{nameof(CABIndexItem.Postcode)}:(\"{keywords}\")",              //phrase-match
                        $"{nameof(CABIndexItem.HiddenText)}:(\"{keywords}\")",            //phrase-match
                        $"{nameof(CABIndexItem.CABNumber)}:(\"{keywords}\")^4",           //phrase-match, boosted x4
                        $"{nameof(CABIndexItem.LegislativeAreas)}:(\"{keywords}\")^6",    //phrase-match, boosted x6
                    };
                    retVal = string.Join(" ", tokens);
                }
            }
            return retVal;
        }
    }
}