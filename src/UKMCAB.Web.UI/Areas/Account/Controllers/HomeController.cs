using Microsoft.AspNetCore.Identity;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account")]
    public class HomeController : Controller
    {
        private readonly SignInManager<UKMCABUser> _signInManager;
        private readonly ILogger<LoginViewModel> _logger;

        public HomeController(SignInManager<UKMCABUser> signInManager, ILogger<LoginViewModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [Route("account/login")]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return Redirect("/");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            var model = new LoginViewModel
            {
                ReturnURL = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [Route("account/login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var user = await _signInManager.UserManager.FindByEmailAsync(loginViewModel.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", Constants.ErrorMessages.InvalidLoginAttempt);
                }
                else if (!user.RequestApproved)
                {
                    ModelState.AddModelError("Email", "Registration request has not yet been approved.");
                }
                else
                {
                    var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, false, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return Redirect(loginViewModel.ReturnURL);
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToAction("Lockout"); 
                    }
                    ModelState.AddModelError("Email", Constants.ErrorMessages.InvalidLoginAttempt);
                    loginViewModel.Password = string.Empty;
                }
            }

            // If we got this far, something failed, redisplay form
            return View(loginViewModel);
        }

        [Route("account/logout")]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return Redirect("/");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            var model = new LogoutViewModel
            {
                ReturnURL = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [Route("account/logout")]
        public async Task<IActionResult> Logout(LogoutViewModel logoutViewModel)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (!string.IsNullOrWhiteSpace(logoutViewModel.ReturnURL))
            {
                return Redirect(logoutViewModel.ReturnURL);
            }
            return View(logoutViewModel);
        }

        [Route("account/lockout")]
        public IActionResult Lockout()
        {
            return View();
        }
    }
}
