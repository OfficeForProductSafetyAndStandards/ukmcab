using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.ApplicationInsights;
using Microsoft.IdentityModel.Abstractions;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UKMCAB.Common;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private SearchClient _indexClient;
        private readonly SearchIndexerClient _searchIndexerClient;
        private readonly TelemetryClient _telemetryClient;

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
                Facets = { $"{nameof(result.BodyTypes)},count:0" , $"{nameof(result.LegislativeAreas)},count:0", $"{nameof(result.RegisteredOfficeLocation)},count:0", $"{nameof(result.TestingLocations)},count:0" }
            });
            if (search.HasValue)
            {
                var facets = search.Value.Facets;

                result.BodyTypes = GetFacetList(facets[nameof(result.BodyTypes)]);
                result.LegislativeAreas = GetFacetList(facets[nameof(result.LegislativeAreas)]).Select(x => x.ToSentenceCase()).ToList()!;
                result.RegisteredOfficeLocation = GetFacetList(facets[nameof(result.RegisteredOfficeLocation)]);
                result.TestingLocations = GetFacetList(facets[nameof(result.TestingLocations)]);
            }

            return result;
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

        private string GetKeywordsQuery(string keywords)
        {
            keywords = keywords?.Trim();
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return "*";
            }
            var specialCharsRegex = new Regex("[+&|\\[!()\\]{}\\^\"~*?:\\/]");
            keywords = specialCharsRegex.Replace(keywords, String.Empty);
            if (string.IsNullOrWhiteSpace(keywords))
            {
                return string.Empty;
            }
            return $"{keywords}"; 
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
            
            if (string.IsNullOrWhiteSpace(query))
            {
                return cabResults;
            }

            var filter = BuildFilter(options);
            var sort = BuildSort(options);

            var searchOptions = new SearchOptions
            {
                Size = options.IgnorePaging ? null : DataConstants.Search.SearchResultsPerPage,
                IncludeTotalCount = true,
                Skip = options.IgnorePaging ? null : DataConstants.Search.SearchResultsPerPage * (options.PageNumber - 1),
                Filter = filter,
                OrderBy = { sort },
                QueryType = SearchQueryType.Simple
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
            if (options.TestingLocationsFilter != null && options.TestingLocationsFilter.Any())
            {
                var testingLocations = string.Join(" or ", options.TestingLocationsFilter.Select(bt => $"TestingLocations/any(tl: tl eq '{bt}')"));
                filters.Add($"({testingLocations})");
            }

            return filters.Count > 1 ? $"({string.Join(" and ", filters)})" : filters.FirstOrDefault() ?? string.Empty;
        }

        public async Task ReIndexAsync()
        {
            var attempts = 0;
            var start = DateTime.UtcNow;
            await _searchIndexerClient.RunIndexerAsync(DataConstants.Search.SEARCH_INDEXER); // this invokes it asynchronously....  you then have to monitor the status to see when it completes
            await Task.Delay(1500); // wait for a bit to give it time to run

            var sw = Stopwatch.StartNew();
            var latest = await GetLastIndexerResultAsync();
            while (true)
            {
                Guard.IsTrue(sw.ElapsedMilliseconds < 30_000, "Azure Cognitive Search is taking an awfully long time to complete indexing");

                var latestEndTime = latest?.EndTime ?? DateTime.MinValue;
                if(latestEndTime > start)
                {
                    break; // the latest indexer completion datetime is _AFTER_ the `RunIndexerAsync` method was called, so can assume it's completed.
                }
                // else - no indexer has completed successfully since this method was first invoked

                latest = await GetLastIndexerResultAsync();
                await Task.Delay(400);
            }
            sw.Stop();
            var completed = DateTime.UtcNow;

            var props = new Dictionary<string, string>
            {
                ["attempts"] = attempts.ToString(),
                ["status"] = latest?.Status.ToString() ?? string.Empty,
                ["elapsed_ms"] = sw.ElapsedMilliseconds.ToString(),
                ["started"] = start.ToString("O"),
                ["completed"] = completed.ToString("O"),
                ["indexer_execution_result"] = latest?.Serialize() ?? string.Empty
            };
            _telemetryClient.TrackEvent("AZCS_REINDEX_COMPLETED", props);

            async Task<IndexerExecutionResult?> GetLastIndexerResultAsync()
            {
                attempts++;
                var status = await _searchIndexerClient.GetIndexerStatusAsync(DataConstants.Search.SEARCH_INDEXER);
                return status.HasValue ? (status?.Value?.LastResult) : null;
            };
        }

        public async Task RemoveFromIndexAsync(string id)
        {
            await _indexClient.DeleteDocumentsAsync("id", new [] { id });
        }
    }
}
