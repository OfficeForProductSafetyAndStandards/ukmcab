using Humanizer;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account"), Route("account"), Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly TelemetryClient _telemetry;
        private readonly IUserService _users;
        private readonly ISecureTokenProcessor _secureTokenProcessor;

        public static class Routes
        {
            public const string Login = "account.login";
            public const string Logout = "account.logout";
            public const string RequestAccount = "account.request";
            public const string RequestAccountSuccess = "account.request.success";
        }

        public AccountController(ILogger<AccountController> logger, TelemetryClient telemetry, IUserService users, ISecureTokenProcessor secureTokenProcessor)
        {
            _logger = logger;
            _telemetry = telemetry;
            _users = users;
            _secureTokenProcessor = secureTokenProcessor;
        }

        [HttpGet("login", Name = Routes.Login)]
        public async Task<IActionResult> LoginAsync()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var envelope = await _users.GetUserAccountStatusAsync(id);

            if (envelope.Status != UserAccountStatus.Active)
            {
                await HttpContext.SignOutAsync();
            }

            switch (envelope.Status)
            {
                case UserAccountStatus.Unknown:
                    var token = _secureTokenProcessor.Enclose(new RequestAccountTokenDescriptor(id, User.FindFirstValue(ClaimTypes.Email)));
                    return RedirectToRoute(Routes.RequestAccount, new { tok = token });
                case UserAccountStatus.PendingUserAccountRequest:
                    throw new DomainException("You have already requested an account. You will receive an email when your request has been reviewed.");
                case UserAccountStatus.UserAccountLocked:
                    throw new DomainException($"Your user account has been {(envelope.UserAccountLockReason == UserAccountLockReason.Archived ? "archived" : "locked")}. Please contact support for assistance.");
                case UserAccountStatus.Active:
                    await _users.UpdateLastLogonDate(id);
                    _telemetry.TrackEvent(AiTracking.Events.LoginSuccess, HttpContext.ToTrackingMetadata());
                    return Redirect("/");
                default:
                    throw new NotSupportedException($"The user account status '{envelope.Status}' is not supported.");
            }
        }

        [AllowAnonymous, HttpGet("request-account", Name = Routes.RequestAccount)]
        public async Task<IActionResult> RequestAccount(string tok)
        { 
            var descriptor = _secureTokenProcessor.Disclose<RequestAccountTokenDescriptor>(tok) ?? throw new DomainException("The token did not deserialize successfully");
            return View(new RequestAccountViewModel { ContactEmailAddress = descriptor.EmailAddress, Token = tok });
        }

        [AllowAnonymous, HttpPost("request-account", Name = Routes.RequestAccount)]
        public async Task<IActionResult> RequestAccountAsync(RequestAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var descriptor = _secureTokenProcessor.Disclose<RequestAccountTokenDescriptor>(model.Token) ?? throw new DomainException("The token did not deserialize successfully");
                await _users.SubmitRequestAccountAsync(new RequestAccountModel
                {
                    SubjectId = descriptor.SubjectId,
                    ContactEmailAddress = model.ContactEmailAddress,
                    EmailAddress = descriptor.EmailAddress,
                    FirstName = model.FirstName,
                    Organisation = model.Organisation,
                    Surname = model.LastName,
                    Comments = model.Comments,
                });
                return RedirectToRoute(Routes.RequestAccountSuccess);
            }
            return View(model);
        }

        [AllowAnonymous, HttpGet("request-account/success", Name = Routes.RequestAccountSuccess)]
        public IActionResult RequestAccountSuccess() => View();


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
