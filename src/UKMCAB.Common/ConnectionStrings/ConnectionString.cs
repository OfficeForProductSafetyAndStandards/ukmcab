namespace UKMCAB.Common.ConnectionStrings;

public abstract class ConnectionString
{
    protected readonly string _dataConnectionString;
    public ConnectionString(string dataConnectionString) => _dataConnectionString = dataConnectionString ?? throw new ArgumentNullException(nameof(dataConnectionString));
    public override string ToString() => _dataConnectionString;
}
