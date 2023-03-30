namespace UKMCAB.Data
{
    public class DataConstants
    {
        public static class CosmosDb
        {
            public const string Database = "main";
            public const string Constainer = "cab-documents";
            public const string ImportContainer = "cab-data";

        }

        public static class Search
        {
            public const int ResultsPerPage = 20;
            public const string SEARCH_INDEX = "ukmcab-search-index";
            public const string SEARCH_INDEXER = "ukmcab-search-indexer";
            public const string SEARCH_DATASOURCE = "ukmcab-search-datasource";

        }
        public class SortOptions
        {
            public const string Default = "default";
            public const string LastUpdated = "lastupd";
            public const string A2ZSort = "a2z";
            public const string Z2ASort = "z2a";
        }

    }
}
