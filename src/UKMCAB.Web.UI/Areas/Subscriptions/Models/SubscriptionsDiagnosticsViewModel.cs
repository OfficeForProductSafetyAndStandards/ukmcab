using System.Collections.Concurrent;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;
using UKMCAB.Web.UI.Services.Subscriptions;

namespace UKMCAB.Web.UI.Areas.Subscriptions.Models;

public class SubscriptionsDiagnosticsViewModel : ILayoutModel
{
    public const string FakeDateTimeFormat = "dd/MM/yyyy HH:mm:ss";
    public SubscriptionsDateTimeProvider.Envelope DateTimeEnvelope { get; internal set; }
    public OutboundEmailSenderMode OutboundSenderMode { get; internal set; }
    public ConcurrentBag<EmailDefinition> SentEmails { get; internal set; }
    public string? LastConfirmationLink { get; internal set; }
    public string? SuccessBannerMessage { get; internal set; }
    public string? FakeDateTime 
    { 
        get 
        {
            string? rv = null;
            if(DateTimeEnvelope.Status == SubscriptionsDateTimeProvider.Status.Fake)
            {
                rv = DateTimeEnvelope.DateTime.ToString(FakeDateTimeFormat);
            }
            return rv;
        }
    }

    public string? Title => "Subscriptions diagnostics";

    public bool IsBackgroundServiceEnabled { get; internal set; }
}
