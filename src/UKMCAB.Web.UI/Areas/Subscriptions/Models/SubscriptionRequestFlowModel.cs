using System.Text.Json.Serialization;
using UKMCAB.Subscriptions.Core.Domain;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionRequestFlowModel
{
    [JsonPropertyName("st")]
    public SubscriptionType? SubscriptionType { get; set; }

    [JsonPropertyName("c")]
    public Guid? CabId { get; set; }

    [JsonPropertyName("cn")]
    public string? CabName { get; set; }

    [JsonPropertyName("q")]
    public string? SearchQueryString { get; set; }

    [JsonPropertyName("kw")]
    public string? Keywords { get; set; }

    [JsonPropertyName("e")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("f")]
    public Frequency? Frequency { get; set; }

    [JsonPropertyName("ru")]
    public string? ReturnUrl { get; set; }
}
