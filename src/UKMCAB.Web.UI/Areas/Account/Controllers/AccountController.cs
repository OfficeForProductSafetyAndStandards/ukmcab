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
            public const string Locked = "account.locked";
            public const string RequestAccount = "account.request";
            public const string RequestAccountSuccess = "account.request.success";
            public const string UserProfile = "account.user.profile";
            public const string EditProfile = "account.edit.profile";
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
                    return RedirectToRoute(Routes.Locked, new { id });
                case UserAccountStatus.Active:
                    await _users.UpdateLastLogonDate(id);
                    _telemetry.TrackEvent(AiTracking.Events.LoginSuccess, HttpContext.ToTrackingMetadata());
                    return Redirect("/");
                default:
                    throw new NotSupportedException($"The user account status '{envelope.Status}' is not supported.");
            }
        }

        [AllowAnonymous]
        [HttpGet("locked/{id}", Name = Routes.Locked)]
        public async Task<IActionResult> AccountLocked(string id)
        {
            var envelope = await _users.GetUserAccountStatusAsync(id);
            if (envelope.Status == UserAccountStatus.UserAccountLocked)
            {
                return View(new BasicPageModel
                {
                    Title = "Account locked"
                });
            }
            return Redirect("/");
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
                    EmailAddress = descriptor.EmailAddress,
                    ContactEmailAddress = model.ContactEmailAddress,
                    FirstName = model.FirstName,
                    Surname = model.LastName,
                    Organisation = model.Organisation,
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

        [HttpGet("user-profile", Name = Routes.UserProfile)]
        public async Task<IActionResult> UserProfile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAccount = await _users.GetAsync(id);
            if (userAccount == null)
            {
                return RedirectToRoute(Routes.Login);
            }
            if (userAccount.IsLocked)
            {
                return RedirectToRoute(Routes.Locked, new { id });
            }

            return View(new UserProfileViewModel
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.Surname,
                PhoneNumber = userAccount.PhoneNumber,
                ContactEmailAddress = userAccount.ContactEmailAddress,
                IsEdited = TempData["Edit"] != null && (bool)TempData["Edit"]
            });
        }

        [HttpGet("edit-profile", Name = Routes.EditProfile)]
        public async Task<IActionResult> EditProfile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAccount = await _users.GetAsync(id);
            if (userAccount == null)
            {
                return RedirectToRoute(Routes.Login);
            }
            if (userAccount.IsLocked)
            {
                return RedirectToRoute(Routes.Locked, new { id });
            }

            return View(new EditProfileViewModel
            {
                FirstName = userAccount.FirstName,
                LastName = userAccount.Surname,
                PhoneNumber = userAccount.PhoneNumber,
                ContactEmailAddress = userAccount.ContactEmailAddress
            });
        }

        [HttpPost("edit-profile", Name = Routes.EditProfile)]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userAccount = await _users.GetAsync(id);
            if (userAccount == null)
            {
                return RedirectToRoute(Routes.Login);
            }
            if (userAccount.IsLocked)
            {
                return RedirectToRoute(Routes.Locked, new { id });
            }

            if (ModelState.IsValid)
            {
                if(!new UserAccountComparer().Equals(userAccount, model.GetUserAccount()))
                {
                    var newUserAccount = userAccount;
                    newUserAccount.FirstName = model.FirstName;
                    newUserAccount.Surname = model.LastName;
                    newUserAccount.PhoneNumber = model.PhoneNumber;
                    newUserAccount.ContactEmailAddress = model.ContactEmailAddress;

                    await _users.UpdateUser(newUserAccount);
                    _telemetry.TrackEvent(AiTracking.Events.UserEditedProfile, HttpContext.ToTrackingMetadata(new Dictionary<string, string>
                    {
                        {"Original user account values", $"FirstName: {userAccount.FirstName}, Surname: {userAccount.Surname}, Phone: {userAccount.PhoneNumber}, ContactEmail: {userAccount.ContactEmailAddress}"},
                        {"New user account values", $"FirstName: {newUserAccount.FirstName}, Surname: {newUserAccount.Surname}, Phone: {newUserAccount.PhoneNumber}, ContactEmail: {newUserAccount.ContactEmailAddress}"},
                    }));

                    TempData.Add("Edit", true);
                }

                return RedirectToRoute(Routes.UserProfile);
            }

            return View(model);
        }
    }

    public class UserAccountComparer : IEqualityComparer<UserAccount>
    {
        public bool Equals(UserAccount x, UserAccount y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.FirstName == y.FirstName && x.Surname == y.Surname && x.ContactEmailAddress == y.ContactEmailAddress && x.PhoneNumber == y.PhoneNumber;
        }

        public int GetHashCode(UserAccount obj)
        {
            return HashCode.Combine(obj.FirstName, obj.Surname, obj.ContactEmailAddress, obj.PhoneNumber);
        }
    }
}
