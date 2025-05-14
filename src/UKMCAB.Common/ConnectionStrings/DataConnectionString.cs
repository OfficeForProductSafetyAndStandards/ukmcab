namespace UKMCAB.Common.ConnectionStrings;

/// <summary>
/// Represents Azure storage (table, blob or queues) connection string
/// </summary>
public class DataConnectionString : ConnectionString
{
    public DataConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(DataConnectionString d) => d._connectionString;
    public static implicit operator DataConnectionString(string d) => new(d);
}
