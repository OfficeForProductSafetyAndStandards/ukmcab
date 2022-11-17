namespace UKMCAB.Common.ConnectionStrings;

public class CosmosDbConnectionString : ConnectionString
{
    public CosmosDbConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(CosmosDbConnectionString d) => d._dataConnectionString;
    public static implicit operator CosmosDbConnectionString(string d) => new(d);
}