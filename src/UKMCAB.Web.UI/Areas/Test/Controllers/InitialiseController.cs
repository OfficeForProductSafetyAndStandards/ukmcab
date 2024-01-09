using Microsoft.ApplicationInsights;
using UKMCAB.Data;
using UKMCAB.Infrastructure.Logging;
using LogEntry = UKMCAB.Infrastructure.Logging.Models.LogEntry;

namespace UKMCAB.Web.UI.Areas.Test.Controllers
{
    [Route("api/[controller]")]    
    [ApiController]
    public class InitialiseController : Controller
    {
        private readonly IInitialiseDataService _initialiseDataService;
        private readonly TelemetryClient _temClient;
        private readonly ILogger<InitialiseController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public InitialiseController(IInitialiseDataService initialiseDataService, TelemetryClient temClient, ILogger<InitialiseController> logger, ILoggingService loggingService, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _initialiseDataService = initialiseDataService;
            _temClient = temClient;
            _logger = logger;
            _loggingService = loggingService;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // check if force initialise data appsetting is enabled and environment is non production
            if (Convert.ToBoolean(_configuration["ForceInitialiseData"]) && !_environment.IsProduction())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _temClient.TrackEvent("init_started");
                        await _initialiseDataService.InitialiseAsync(true);                        
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
            else
            {
                await Task.CompletedTask;
                return NoContent();
            }           
        }
    }
}
