using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using UKMCAB.Common.ConnectionStrings;

namespace UKMCAB.Data.CosmosDb;
public static class CosmosClientFactory
{
    public static CosmosClient Create(CosmosDbConnectionString connectionString) => new CosmosClientBuilder(connectionString)
        .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
        .Build();
}
