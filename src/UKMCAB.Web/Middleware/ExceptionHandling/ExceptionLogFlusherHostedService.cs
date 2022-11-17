using Microsoft.Extensions.Hosting;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Web.Middleware.ExceptionHandling;

public class ExceptionLogFlusherHostedService : BackgroundService
{
    private const int Delay = 2000; // wait N secs between invocations
    private readonly ILoggingService _loggingService;

    public ExceptionLogFlusherHostedService(ILoggingService loggingService) => _loggingService = loggingService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _loggingService.FlushAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _loggingService.Log(new LogEntry(ex));
            }

            await Task.Delay(Delay, stoppingToken);
        }
    }
}
