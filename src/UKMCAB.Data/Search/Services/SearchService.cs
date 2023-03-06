using Azure.Search.Documents;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private SearchClient _indexClient;
        private readonly int SearchResultPerPage;

        public SearchService(SearchClient searchClient, int searhchResultPerPage)
        {
            _indexClient = searchClient;
            SearchResultPerPage = searhchResultPerPage;
        }

        public async Task<FacetResult> GetFacetsAsync()
        {
            var result = new FacetResult();
            var search = await _indexClient.SearchAsync<CABIndexItem>("*", new SearchOptions
            {
                Facets = { nameof(result.BodyTypes), nameof(result.LegislativeAreas), nameof(result.RegisteredOfficeLocation), nameof(result.TestingLocations) }
            });
            if (search.HasValue)
            {
                var facets = search.Value.Facets;

                result.BodyTypes = facets[nameof(result.BodyTypes)].Select(f => f.Value.ToString()).ToList();
                result.LegislativeAreas = facets[nameof(result.LegislativeAreas)].Select(f => f.Value.ToString()).ToList();
                result.RegisteredOfficeLocation = facets[nameof(result.RegisteredOfficeLocation)].Select(f => f.Value.ToString()).ToList();
                result.TestingLocations = facets[nameof(result.TestingLocations)].Select(f => f.Value.ToString()).ToList();
            }

            return result;
        }

        public async Task<CABResults> QueryAsync(CABSearchOptions options)
        {
            var query = string.IsNullOrWhiteSpace(options.Keywords) ? "*" : options.Keywords;
            var filter = BuildFilter(options);
            var sort = BuildSort(options);
            var search = await _indexClient.SearchAsync<CABIndexItem>(query, new SearchOptions
            {
                Size = SearchResultPerPage,
                IncludeTotalCount = true,
                Skip = SearchResultPerPage * (options.PageNumber - 1),
                Filter = filter,
                OrderBy = { sort }
            });
            var cabResults = new CABResults
            {
                PageNumber = options.PageNumber,
                Total = 0,
                CABs = new List<CABIndexItem>()
            };
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
            if (options.Sort == "lastupd")
            {
                return "LastUpdatedDate asc";
            }
            if (options.Sort == "a2z")
            {
                return "Name asc";
            }
            if (options.Sort == "z2a")
            {
                return "Name desc";
            }

            return string.IsNullOrWhiteSpace(options.Keywords) ? "RandomSort asc" : string.Empty;
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
    }
}
