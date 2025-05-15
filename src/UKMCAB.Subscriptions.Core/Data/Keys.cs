namespace UKMCAB.Subscriptions.Core.Data;
public class Keys
{
    public string? TableKey { get; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }

    public Keys(string? tableKey, string? partitionKey, string? rowKey)
    {
        TableKey = tableKey;
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public Keys(string composite) : this(composite.Split('$')) { }

    private Keys(string[] composite) : this(composite[0], composite[1], composite[2]) { }

    public override string ToString() => string.Concat(TableKey, '$', PartitionKey, '$', RowKey);

    public static implicit operator string(Keys d) => d.ToString();

    public static implicit operator Keys(string d) => new(d);
}
