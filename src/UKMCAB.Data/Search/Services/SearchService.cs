using System.Text.RegularExpressions;
using UKMCAB.Common;
using UKMCAB.Data.Search.Models;
using OpenSearch.Client;
using System.Linq.Expressions;
using OpenSearch.Net;

namespace UKMCAB.Data.Search.Services
{
    public class SearchService : ISearchService
    {
        private readonly IOpenSearchClient _openSearchClient;
        public SearchService(IOpenSearchClient openSearchClient)
        {
            _openSearchClient = openSearchClient;
        }

        public async Task<SearchFacets> GetFacetsAsync(bool internalSearch)
        {
            var result = new SearchFacets();

            var response = await _openSearchClient.SearchAsync<CABIndexItem>(s => s
                .Size(0)
                .Query(q =>
                    internalSearch
                        ? q.MatchAll()
                        // TODO: Investigate Why is StatusValue no longer number. Postgres???
                        //: q.Bool(b => b.Filter(f => f.Terms(t => t.Field(fld => fld.StatusValue).Terms("30", "40"))))
                        : q.Bool(b => b.Filter(f => f.Terms(t => t.Field(fld => fld.StatusValue).Terms("Published", "Archived"))))
                )
                .Aggregations(aggs => aggs
                    .Terms("BodyTypes", t => t.Field(f => f.BodyTypes))
                    .Terms("MRACountries", t => t.Field("mRACountries"))
                    .Terms("RegisteredOfficeLocation", t => t.Field(f => f.RegisteredOfficeLocation))
                    .Terms("StatusValue", t => t.Field("statusValue"))
                    .Terms("CreatedByUserGroup", t => t.Field(f => f.CreatedByUserGroup))
                    .Terms("SubStatus", t => t.Field(f => f.SubStatus))

                    .Nested("DocumentLegislativeAreas", n => n
                        .Path("documentLegislativeAreas")
                        .Aggregations(nested => nested
                            .Terms("LegislativeAreas", t => t.Field("documentLegislativeAreas.legislativeAreaName.keyword"))
                            .Terms("ProvisionalLegislativeAreas", t => t.Field("documentLegislativeAreas.isProvisional"))
                            .Terms("LegislativeAreaStatus", t => t.Field("documentLegislativeAreas.archived"))
                            .Terms("LAStatus", t => t.Field("documentLegislativeAreas.status"))
                        )
                    )

                )
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to retrive facets: {response.ServerError?.Error?.Reason}");
            }

            var aggs = response.Aggregations;

            result.BodyTypes = GetFacetList(aggs.Terms("BodyTypes"));
            result.MRACountries = GetFacetList(aggs.Terms("MRACountries"));
            result.RegisteredOfficeLocation = GetFacetList(aggs.Terms("RegisteredOfficeLocation"));
            result.StatusValue = GetFacetList(aggs.Terms("StatusValue"));
            result.CreatedByUserGroup = GetFacetList(aggs.Terms("CreatedByUserGroup"));
            result.SubStatus = GetFacetList(aggs.Terms("SubStatus"));

            var nestedAggs = aggs.Nested("DocumentLegislativeAreas");
            result.LegislativeAreas = GetFacetList(nestedAggs.Terms("LegislativeAreas")).ToList();
            result.ProvisionalLegislativeAreas = GetFacetList(nestedAggs.Terms("ProvisionalLegislativeAreas")).OrderBy(x => x).ToList();
            result.LegislativeAreaStatus = GetFacetList(nestedAggs.Terms("LegislativeAreaStatus")).OrderBy(x => x).ToList();
            result.LAStatus = GetFacetList(nestedAggs.Terms("LAStatus"));

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

            var keywordQuery = GetKeywordsQuery(options.Keywords ?? string.Empty, options.InternalSearch);
            var filterQuery = BuildFilterQuery(options);
            var sort = BuildSortQuery(options);

            var finalQuery = keywordQuery != null && filterQuery != null
                ? new BoolQuery
                {
                    Must = new List<QueryContainer> { keywordQuery, filterQuery }
                }
                : keywordQuery ?? filterQuery;

            var from = options.IgnorePaging
                ? 0
                : DataConstants.Search.SearchResultsPerPage * (options.PageNumber - 1);

            var size = options.IgnorePaging
                ? 1000
                : DataConstants.Search.SearchResultsPerPage;

            var searchRequest = new SearchRequest<CABIndexItem>
            {
                From = from,
                Size = size,
                Query = finalQuery,
                Sort = sort,
                TrackTotalHits = true,
                Source = options.Select != null && options.Select.Any()
                    ? new SourceFilter { Includes = options.Select.ToArray() }
                    : null
            };

            var testResponse = await _openSearchClient.SearchAsync<CABIndexItem>(s => s.Query(_ => finalQuery));

            var response = await _openSearchClient.SearchAsync<CABIndexItem>(s => s
                .From(from)
                .Size(size)
                .Query(_ => finalQuery)
                // TODO: Fix Sort and Source
                //.Sort(sort)
                .TrackTotalHits()
                //.Source(sf =>
                //{
                //    if (options.Select != null && options.Select.Any())
                //    {
                //        sf.Includes(f => f.Fields(options.Select.ToArray()));
                //    }
                //    return sf;
                //})
            );

            if (!response.IsValid || !response.Documents.Any())
            {
                return cabResults;
            }

            cabResults.CABs = response.Documents.ToList();
            cabResults.Total = (int)response.Total;

            return cabResults;
        }

        private QueryContainer? BuildFilterQuery(CABSearchOptions options)
        {
            QueryContainer query = new QueryContainer();

            if(options.BodyTypesFilter?.Any() == true)
            {
                query &= new TermsQuery
                {
                    Field = "bodyTypes",
                    Terms = options.BodyTypesFilter?.Cast<object>().ToList()
                };
            }
            // TODO: Add other filters
            if (options.LegislativeAreasFilter?.Any() == true)
            {
                var laQueries = options.LegislativeAreasFilter.Select(la =>
                    new BoolQuery
                    {
                        Must = new List<QueryContainer>
                        {
                            new TermQuery
                            {
                                Field = "documentLegislativeAreas.legislativeAreaName.keyword",
                                Value = la
                            },
                            !options.InternalSearch
                                ? new TermQuery
                                {
                                    Field = "documentLegislativeAreas.archived",
                                    Value = false
                                }
                                : null
                        }.Where(q => q != null).ToList()
                    });

                query &= new NestedQuery
                {
                    Path = "documentLegislativeAreas",
                    Query = new BoolQuery
                    {
                        Should = laQueries.Select(q => (QueryContainer)q).ToList(),
                        MinimumShouldMatch = 1
                    }
                };
            }

            if (!options.IsOPSSUser)
            {
                query &= new BoolQuery
                {
                    MustNot = new List<QueryContainer>
                    {
                        new BoolQuery
                        {
                            Must = new List<QueryContainer>
                            {
                                new TermQuery{Field = "createdByUserGroup.keyword", Value = "opss"},
                                new TermQuery{Field = "statusValue", Value = "Draft" }
                                // TODO: Fix issue with Status value not being numbers eg. "30"
                                //new TermQuery{Field = "statusValue", Value = (int)Status.Draft}
                            }
                        }
                    }
                };
            }

            return query;
        }

        // TODO: Change Sort query signature.
        //private Func<SortDescriptor<CABIndexItem>, IPromise<IList<ISort>>> BuildSortQuery(CABSearchOptions options)
        private List<ISort> BuildSortQuery(CABSearchOptions options)
        {
            var sortList = new List<ISort>();

            switch (options.Sort)
            {
                case DataConstants.SortOptions.LastUpdated:
                    sortList.Add(new FieldSort { Field = "lastUpdatedDate", Order = SortOrder.Descending});
                    break;

                case DataConstants.SortOptions.A2ZSort:
                    sortList.Add(new FieldSort { Field = "name.keyword", Order = SortOrder.Ascending });
                    break;

                case DataConstants.SortOptions.Z2ASort:
                    sortList.Add(new FieldSort { Field = "name.keyword", Order = SortOrder.Descending });
                    break;

                case DataConstants.SortOptions.Default:
                default:
                    if (string.IsNullOrWhiteSpace(options.Keywords))
                    {
                        sortList.Add(new FieldSort { Field = "randomSort", Order = SortOrder.Ascending });
                    }
                    break;
            }
            return sortList;
        }
        public async Task ReIndexAsync(CABIndexItem cabIndexItem)
        {
            var response = await _openSearchClient.IndexAsync(cabIndexItem, s => s
                .Index(DataConstants.Search.SEARCH_INDEX)
                .Id(cabIndexItem.CABId)
                .Refresh(Refresh.True)
            );

            Guard.IsTrue(response.IsValid, $"Failed to update index for {cabIndexItem.CABId}: {response.OriginalException?.Message}");
        }

        public async Task RemoveFromIndexAsync(string id)
        {
            var response = await _openSearchClient.DeleteAsync<CABIndexItem>(id, d => d
                .Index(DataConstants.Search.SEARCH_INDEX)
                .Refresh(Refresh.True)
            );

            Guard.IsTrue(response.IsValid, $"Failed to remove index document with {id}: {response.DebugInformation}");
        }

        private List<string> GetFacetList(TermsAggregate<string> bucket)
        {
            return bucket.Buckets
                    .Select(b => b.Key)
                    .Where(k => k != null)
                    .ToList();
        }

        // TODO: Sort method out
        private IEnumerable<string> GetLegislativeAreaStatusFacetList(TermsAggregate<string> bucket)
            => GetFacetList(bucket).Select(f => f).OrderBy(f => f).ToList();
        
        //private IEnumerable<string> GetLegislativeAreaStatusFacetList(IEnumerable<FacetResult> facets)
        //    => facets.Select(f => f.Value.ToString()).OrderBy(f => f).ToList();
        
        private static readonly Regex SpecialCharsRegex = new("[+\\-&|\\[!()\\]{}\\^\"~*?:\\/]");

        private QueryContainer GetKeywordsQuery(string? keywords, bool internalSearch)
        {
            var input = (keywords ?? string.Empty).Trim();
            if (input.Contains("~lucene"))
            {
                var rawQuery = input.Replace("~lucene", string.Empty).Trim();
                return new QueryStringQuery
                {
                    Query = rawQuery
                };
            }

            input = SpecialCharsRegex.Replace(input, " ");

            if (string.IsNullOrWhiteSpace(input.Clean()))
            {
                return new MatchAllQuery();
            }

            var queries = new List<QueryContainer>();
            var fields = new List<(Expression<Func<CABIndexItem, object>> Field, double Boost)>
            {
                (x => x.Name, 3),
                (x => x.TownCity, 1),
                (x => x.Postcode, 1),
                (x => x.HiddenText, 1),
                (x => x.CABNumber, 4),
                (x => x.PreviousCABNumbers, 4),
                (x => x.HiddenScopeOfAppointments, 6),
                (x => x.UKASReference, 1),
            };

            var container = new QueryContainer();

            foreach (var (field, boost) in fields) 
            {
                container |= new MatchQuery
                {
                    Field = field,
                    Query = input,
                    Boost = (float?)boost
                };
            }

            var nestedQuery = new NestedQuery
            {
                Path = "documentLegislativeAreas",
                Query = new MatchQuery
                {
                    Field = "documentLegislativeAreas.legislativeAreaName",
                    Query = input,
                    Boost = 6
                }
            };

            container |= nestedQuery;

            if (!internalSearch)
            {
                container &= new BoolQuery
                {
                    Should = new List<QueryContainer>
                    {
                        new TermQuery {Field = "statusValue", Value = "30"},
                        new TermQuery {Field = "statusValue", Value = "40"}
                    },
                    MinimumShouldMatch = 1
                };
            }

            return container;
        }
    }
}