using Amazon;
using Amazon.Runtime;
using OpenSearch.Client;
using OpenSearch.Net;

namespace UKMCAB.Data.Search.Services
{
    public class PostgreSearchServiceManagment : ISearchServiceManagment
    {
        public async Task InitialiseAsync(bool force = false)
        {
            var uri = new Uri("http://ukmcab-opensearch:9200/");

            var connectionPool = new SingleNodeConnectionPool(uri);

            var indexName = DataConstants.Search.SEARCH_INDEX;

            var settings = new OpenSearch.Client.ConnectionSettings(connectionPool)
                .DefaultIndex(indexName);

            var client = new OpenSearchClient(settings);

            //ping cluster
            var pingResponse = await client.PingAsync();
        }
    }
}
