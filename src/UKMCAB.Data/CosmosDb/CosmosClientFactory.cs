using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Data.CosmosDb;
public static class CosmosClientFactory
{
    public static CosmosClient Create(ConnectionString connectionString) => new CosmosClientBuilder(connectionString.ToString())
        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
        .Build();
}
