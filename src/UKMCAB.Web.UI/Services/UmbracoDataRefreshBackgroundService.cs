using UKMCAB.Data;

namespace UKMCAB.Web.UI.Services;
public class UmbracoDataRefreshBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(30_000, stoppingToken);
            await CabRepository.LoadAsync();
        }
    }
}
