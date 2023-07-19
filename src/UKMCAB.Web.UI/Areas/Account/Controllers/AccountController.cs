using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account"), Route("account"), Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<LoginViewModel> _logger;
        private readonly TelemetryClient _telemetry;

        public static class Routes
        {
            public const string Login = "account.login";
            public const string Logout = "account.logout";
        }

        public AccountController(ILogger<LoginViewModel> logger, TelemetryClient telemetry)
        {
            _logger = logger;
            _telemetry = telemetry;
        }

        [HttpGet("login", Name = Routes.Login)]
        public IActionResult Login()
        {
            return Redirect("/");
        }


        [HttpGet("logout", Name = Routes.Logout)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _logger.LogInformation("User logged out.");
            _telemetry.TrackEvent(AiTracking.Events.Logout, HttpContext.ToTrackingMetadata());
            return Redirect("/");
        }
    }
}
