using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Abstractions;
using UKMCAB.Data;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Infrastructure.Logging.Models;
using LogEntry = UKMCAB.Infrastructure.Logging.Models.LogEntry;

namespace UKMCAB.Web.UI.Areas.Test.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    [ApiController]
    public class InitialiseController : Controller
    {
        private readonly IInitialiseDataService _initialiseDataService;
        private readonly TelemetryClient _temClient;
        private readonly ILogger<InitialiseController> _logger;
        private readonly ILoggingService _loggingService;

        public InitialiseController(IInitialiseDataService initialiseDataService, TelemetryClient temClient, ILogger<InitialiseController> logger, ILoggingService loggingService)
        {
            _initialiseDataService = initialiseDataService;
            _temClient = temClient;
            _logger = logger;
            _loggingService = loggingService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var cutOffDate = new DateTime(2023, 5, 27);

            _ = Task.Run(async () =>
            {
                try
                {
                    if (DateTime.UtcNow < cutOffDate)
                    {
                        await _initialiseDataService.InitialiseAsync(true);
                    }
                    _temClient.TrackEvent("init_complete");
                }
                catch (Exception e)
                {
                    _temClient.TrackException(e);
                    _logger.LogError(e, "Initialisation failed");
                    _loggingService.Log(new LogEntry(e));
                }
            });

            return Accepted();
        }
    }
}
