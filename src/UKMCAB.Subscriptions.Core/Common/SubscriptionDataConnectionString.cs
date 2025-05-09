namespace UKMCAB.Subscriptions.Core.Common;

/// <summary>
/// The core options that need to be configured for this package to function
/// </summary>
public class SubscriptionDataConnectionString : ConnectionString
{
    public SubscriptionDataConnectionString(string dataConnectionString) : base(dataConnectionString) { }
    public static implicit operator string(SubscriptionDataConnectionString d) => d._connectionString;
    public static implicit operator SubscriptionDataConnectionString(string d) => new(d);
}