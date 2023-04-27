using System.Text.Json.Serialization;
using UKMCAB.Subscriptions.Core;
using static UKMCAB.Web.UI.Services.Subscriptions.SubscriptionsDateTimeProvider;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public interface ISubscriptionsDateTimeProvider
{
    void Clear();
    Envelope Get();
    void Set(SetPayload payload);
}

/// <summary>
/// Primary use is to allow the Subscriptions Core to get the current datetime.
/// However, this class also allows for the current datetime used by Subscriptions Core to be manipulated for a period of time. Useful for QA E2E testing.
/// </summary>
public class SubscriptionsDateTimeProvider : IDateTimeProvider, ISubscriptionsDateTimeProvider
{
    private readonly RealDateTimeProvider _real;

    private DateTime? _fakeDateTime;   // the current fake datetime
    private DateTime? _expires;        // when does the fake datetime expire (after which, the system will revert to real datetime)

    public enum Status { Fake, Real }

    public class SetPayload
    {
        public DateTime? DateTime { get; set; }
        public int ExpiryHours { get; set; }
    }

    public class Envelope
    {
        public DateTime DateTime { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))] 
        public Status Status { get; set; }

        public Envelope(DateTime dateTime, Status status)
        {
            DateTime = dateTime;
            Status = status;
        }
    }

    public SubscriptionsDateTimeProvider() => _real = new RealDateTimeProvider();

    public DateTime UtcNow => Get().DateTime;

    public void Set(SetPayload payload)
    {
        _fakeDateTime = payload.DateTime?.AsUtc();
        if(_fakeDateTime != null)
        {
            _expires = DateTime.UtcNow.AddHours(payload.ExpiryHours);
        }
    }

    public void Clear()
    {
        _expires = null;
        _fakeDateTime = null;
    }

    public Envelope Get()
    {
        if (_fakeDateTime != null && _expires < _real.UtcNow)
        {
            Clear();
        }

        if (_fakeDateTime != null)
        {
            return new(_fakeDateTime.Value, Status.Fake);
        }
        else
        {
            return new(_real.UtcNow, Status.Real);
        }
    }
}
