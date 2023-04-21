using Microsoft.ApplicationInsights;
using System.Diagnostics;
using System.Text.Json;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Subscriptions.Core.Services;
using static UKMCAB.Web.UI.Services.Subscriptions.SubscriptionEngineCoordinator;

namespace UKMCAB.Web.UI.Services.Subscriptions;

public interface ISubscriptionEngineCoordinator
{
    Task<RequestResult> RequestProcessAsync(CancellationToken stoppingToken);
}

public class SubscriptionEngineCoordinator : ISubscriptionEngineCoordinator
{
    private readonly ISubscriptionEngine _subscriptionEngine;
    private readonly TelemetryClient _telemetryClient;
    private readonly IDistCache _distCache;

    public enum Result { Success, Error, Concurrent }

    public record RequestResult(string Status, SubscriptionEngine.ResultAccumulator? Stats = null, Exception? Exception = null);

    public SubscriptionEngineCoordinator(ISubscriptionEngine subscriptionEngine, TelemetryClient telemetryClient, IDistCache distCache)
    {
        _subscriptionEngine = subscriptionEngine;
        _telemetryClient = telemetryClient;
        _distCache = distCache;
    }

    public async Task<RequestResult> RequestProcessAsync(CancellationToken stoppingToken)
    {
        if (!_subscriptionEngine.CanProcess())
        {
            return new RequestResult("Cannot process. Uri templates not initialised");
        }

        var lockName = StringExt.Keyify(nameof(SubscriptionEngineCoordinator), nameof(RequestProcessAsync));
        var lockOwner = LockOwner.Create();
        try
        {
            var got = await _distCache.LockTakeAsync(lockName, lockOwner, TimeSpan.FromMinutes(1));
            if (got)
            {
                var results = await ProcessAsync(stoppingToken).ConfigureAwait(false);
                return new(Result.Success.ToString(), results);
            }
            else
            {
                return new(Result.Concurrent.ToString());
            }
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            return new(Result.Error.ToString(), null, ex);
        }
        finally
        {
            await _distCache.LockReleaseAsync(lockName, lockOwner);
        }
    }

    private async Task<SubscriptionEngine.ResultAccumulator> ProcessAsync(CancellationToken stoppingToken)
    {
        var sw = Stopwatch.StartNew();
        var results = await _subscriptionEngine.ProcessAsync(stoppingToken).ConfigureAwait(false);
        sw.Stop();

        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(results))?
            .ToDictionary(x => x.Key, x => x.Value.ToString())
            ?? new Dictionary<string, string?>();

        properties.Add("Elapsed", sw.Elapsed.ToString());
        _telemetryClient.TrackEvent("SUBSCRIPTIONS_PROCESSED", properties);

        return results;
    }
}