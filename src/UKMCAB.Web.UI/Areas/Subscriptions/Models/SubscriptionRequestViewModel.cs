using System.Text.Json.Serialization;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public enum SubscriptionType
{
    Search,
    Cab
}

public class SubscriptionRequestViewModel
{
    [JsonPropertyName("st")]
    public SubscriptionType? SubscriptionType { get; set; }

    [JsonPropertyName("c")]
    public Guid? CabId { get; set; }

    [JsonPropertyName("q")]
    public string? SearchQueryString { get; set; }

    [JsonPropertyName("e")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("f")]
    public Frequency? Frequency { get; set; }


}
