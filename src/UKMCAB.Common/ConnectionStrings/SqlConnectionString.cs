namespace UKMCAB.Common.ConnectionStrings;

public class SqlConnectionString : ConnectionString
{
    public SqlConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(SqlConnectionString d) => d._connectionString;
    public static implicit operator SqlConnectionString(string d) => new(d);
}
