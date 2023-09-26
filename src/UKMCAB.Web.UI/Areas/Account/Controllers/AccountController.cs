using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Security.Tokens;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Users.Models;
using UKMCAB.Data.CosmosDb.Services.User;
using UKMCAB.Data.Models.Users;
using UKMCAB.Web.Security;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Models.ViewModels;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account"), Route("account"), Authorize]
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AccountController> _logger;
        private readonly ISecureTokenProcessor _secureTokenProcessor;
        private readonly TelemetryClient _telemetry;
        private readonly IUserAccountRepository _userAccounts;
        private readonly IUserService _users;
        public AccountController(ILogger<AccountController> logger, TelemetryClient telemetry, IUserService users, ISecureTokenProcessor secureTokenProcessor, IWebHostEnvironment environment, IUserAccountRepository userAccounts)
        {
            _logger = logger;
            _telemetry = telemetry;
            _users = users;
            _secureTokenProcessor = secureTokenProcessor;
            _environment = environment;
            _userAccounts = userAccounts;
        }

        [AllowAnonymous]
        [HttpGet("locked/{id}", Name = Routes.Locked)]
        public async Task<IActionResult> AccountLocked(string id)
        {
            var envelope = await _users.GetUserAccountStatusAsync(id);
            if (envelope.Status == UserAccountStatus.UserAccountLocked)
            {
                return View(new AccountLockedViewModel
                {
                    Title = "Account locked",
                    Reason = envelope.UserAccountLockReason,
                });
            }
            return Redirect("/");
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
                if (!new UserAccountComparer().Equals(userAccount, model.GetUserAccount()))
                {
                    var newUserAccount = userAccount;
                    newUserAccount.FirstName = model.FirstName;
                    newUserAccount.Surname = model.LastName;
                    newUserAccount.ContactEmailAddress = model.ContactEmailAddress;

                    await _users.UpdateUser(newUserAccount);
                    _telemetry.TrackEvent(AiTracking.Events.UserEditedProfile, HttpContext.ToTrackingMetadata(new Dictionary<string, string>
                    {
                        {"Original user account values", $"FirstName: {userAccount.FirstName}, Surname: {userAccount.Surname}, ContactEmail: {userAccount.ContactEmailAddress}"},
                        {"New user account values", $"FirstName: {newUserAccount.FirstName}, Surname: {newUserAccount.Surname}, ContactEmail: {newUserAccount.ContactEmailAddress}"},
                    }));

                    TempData.Add("Edit", true);
                }

                return RedirectToRoute(Routes.UserProfile);
            }

            return View(model);
        }

        [AllowAnonymous, Route("fakelogin", Name = Routes.FakeLogin)]
        public async Task<IActionResult> FakeLogin([FromForm] string role)
        {
            if (_environment.IsDevelopment())
            {
                if (Request.Method == HttpMethod.Get.Method)
                {
                    return View();
                }
                else if (Request.Method == HttpMethod.Post.Method)
                {
                    var claimsIdentity = new ClaimsIdentity(Enumerable.Empty<Claim>(), CookieAuthenticationDefaults.AuthenticationScheme);
                    var acc = new UserAccount
                    {
                        FirstName = "Fake",
                        Surname = $"Persona ({Roles.NameFor(role)})",
                        Role = role,
                        Id = $"fake_{Guid.NewGuid()}"
                    };

                    SignInHelper.AddClaims(acc, claimsIdentity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = false, });

                    return Redirect("/");
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                return NotFound();
            }
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

                    var uri = Url.RouteUrl(HomeController.Routes.Message, new
                    {
                        token = _secureTokenProcessor.Enclose(new MessageViewModel
                        {
                            Title = "Account not approved",
                            Heading = "Account not approved",
                            Body = "You have already requested an account. You will receive an email when your request has been reviewed.",
                            ViewName = "Message",
                        })
                    });
                    return RedirectToAction(nameof(LogoutAndRedirect), new { uri });


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

        [HttpGet("logout-redirect"), AllowAnonymous]
        public async Task<IActionResult> LogoutAndRedirect(string uri)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(uri);
        }


        [HttpGet("logout", Name = Routes.Logout)]
        public async Task<IActionResult> Logout()
        {
            void Track()
            {
                _logger.LogInformation("User logged out.");
                _telemetry.TrackEvent(AiTracking.Events.Logout, HttpContext.ToTrackingMetadata());
            }

            if (User.HasClaim(x => x.Type == Claims.IsOneLoginUser))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                Track();
                return new EmptyResult();
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                Track();
                return Redirect("/");
            }
        }

        [AllowAnonymous, Route("qalogin", Name = Routes.QaLogin)]
        public async Task<IActionResult> QaLogin([FromForm] string userId)
        {
            if (_environment.IsDevelopment())
            {
                if (Request.Method == HttpMethod.Get.Method)
                {
                    return Content(@"
                    <html><body><h1>QA fake login</h1>
                    <form method=post>
                        User ID: <input name=userid>
                        <input type=submit />
                        <p>If a user id is unrecognised, you'll go through the user account request flow.  If it's recognised, then great, you'll be logged on as that account.</p>
                    </form>
                    </html></body>", "text/html");
                }
                else if (Request.Method == HttpMethod.Post.Method)
                {
                    var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, CookieAuthenticationDefaults.AuthenticationScheme);
                    var acc = await _userAccounts.GetAsync(userId);
                    if (acc != null)
                    {
                        SignInHelper.AddClaims(acc, claimsIdentity);
                    }

                    var authProperties = new AuthenticationProperties { IsPersistent = false, };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToRoute(Routes.Login);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                return NotFound();
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
                OrganisationName = userAccount.OrganisationName,
                ContactEmailAddress = userAccount.ContactEmailAddress,
                Role = userAccount.Role,
                LastLogonUtc = userAccount.LastLogonUtc,
                IsEdited = TempData["Edit"] != null && (bool)TempData["Edit"]
            });
        }

        public static class Routes
        {
            public const string EditProfile = "account.edit.profile";
            public const string FakeLogin = "account.fakelogin";
            public const string Locked = "account.locked";
            public const string Login = "account.login";
            public const string Logout = "account.logout";
            public const string QaLogin = "account.qalogin";
            public const string RequestAccount = "account.request";
            public const string RequestAccountSuccess = "account.request.success";
            public const string UserProfile = "account.user.profile";
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
            return x.FirstName == y.FirstName && x.Surname == y.Surname && x.ContactEmailAddress == y.ContactEmailAddress;
        }

        public int GetHashCode(UserAccount obj)
        {
            return HashCode.Combine(obj.FirstName, obj.Surname, obj.ContactEmailAddress);
        }
    }
}