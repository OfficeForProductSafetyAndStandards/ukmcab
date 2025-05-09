using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Data.Models;

namespace UKMCAB.Subscriptions.Core.Data;

public interface ITelemetryRepository : IRepository
{
    Task TrackAsync(string key, string text);
    Task TrackByEmailAddressAsync(string emailAddress, string text);
}

public class TelemetryRepository : Repository<TableEntity>, ITelemetryRepository
{
    private const string tableKey = $"{SubscriptionsCoreServicesOptions.TableNamePrefix}telemetry";
    public TelemetryRepository(SubscriptionDbContext dataContext) : base(dataContext, tableKey) { }

    public async Task TrackByEmailAddressAsync(string emailAddress, string text)
    {
        await Task.WhenAll(
            UpsertAsync(new TableEntity(tableKey, emailAddress, Timestamp.Get()).Pipe(x => x.Add("Text", text))),
            UpsertAsync(new TableEntity(tableKey, "global", Timestamp.Reverse()).Pipe(x => x.Add("Text", text), x => x.Add("EmailAddress", emailAddress))));
    }

    public async Task TrackAsync(string key, string text) => await UpsertAsync(new TableEntity(tableKey, key, Timestamp.Get()).Pipe(x => x.Add("Text", text)));
}


