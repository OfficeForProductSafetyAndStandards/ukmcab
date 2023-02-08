namespace UKMCAB.Common.ConnectionStrings;

/// <summary>
/// Useful for connection strings that contain equals signs in its value.
/// </summary>
public abstract class ConnectionStringBase
{
    public string? Value { get; private set; }
    protected Part[] Parts { get; set; } = Array.Empty<Part>();

    public ConnectionStringBase(string? connectionString)
    {
        if (connectionString != null)
        {
            Value = connectionString;
            const string DoubleEquals = "==";
            const string QuadrupleTildes = "~~~~";

            connectionString = connectionString.Replace(DoubleEquals, QuadrupleTildes);

            Parts = connectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split('='))
                .Select(x => new Part { Name = x[0], Value = x[1].Replace(QuadrupleTildes, DoubleEquals) }).ToArray();
        }
    }

    protected class Part
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
    }
}
