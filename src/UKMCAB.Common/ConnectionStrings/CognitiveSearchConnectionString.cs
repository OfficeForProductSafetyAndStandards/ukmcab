namespace UKMCAB.Common.ConnectionStrings;

/// <summary>
/// Represents a connection string for Azure Cognitive Search
/// </summary>
public class CognitiveSearchConnectionString : ConnectionStringBase
{
    public CognitiveSearchConnectionString(string? connectionString) : base(connectionString) { }

    public string Endpoint => Parts.SingleOrDefault(x => x.Name.DoesEqual(nameof(Endpoint)))?.Value ?? throw new Exception($"Error: {nameof(Endpoint)} is empty");

    public string ApiKey => Parts.SingleOrDefault(x => x.Name.DoesEqual(nameof(ApiKey)))?.Value ?? throw new Exception($"Error: {nameof(ApiKey)} is empty");
}