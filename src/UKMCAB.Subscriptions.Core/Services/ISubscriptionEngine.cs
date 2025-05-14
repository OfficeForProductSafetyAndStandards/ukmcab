namespace UKMCAB.Subscriptions.Core.Services;

public interface ISubscriptionEngine
{
    bool CanProcess();
    Task<SubscriptionEngine.ResultAccumulator> ProcessAsync(CancellationToken cancellationToken);
}
