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
    }
}
