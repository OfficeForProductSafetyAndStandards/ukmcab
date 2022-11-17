namespace UKMCAB.Common.ConnectionStrings;

/// <summary>
/// Represents Azure storage (table, blob or queues) connection string
/// </summary>
public class AzureDataConnectionString : ConnectionString
{
    public AzureDataConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(AzureDataConnectionString d) => d._dataConnectionString;
    public static implicit operator AzureDataConnectionString(string d) => new(d);
}
