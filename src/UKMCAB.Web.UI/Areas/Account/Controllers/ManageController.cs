using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account")]
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly SignInManager<UKMCABUser> _signInManager;
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly TelemetryClient _telemetry;
        private readonly TemplateOptions _templateOptions;

        public ManageController(UserManager<UKMCABUser> userManager, SignInManager<UKMCABUser> signInManager, IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions, TelemetryClient telemetry) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _asyncNotificationClient = asyncNotificationClient;
            _telemetry = telemetry;
            _templateOptions = templateOptions.Value;
        }

        [Route("account/manage/change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            Guard.IsFalse<PermissionDeniedException>(user == null);

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Route("account/manage/change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                Guard.IsFalse<PermissionDeniedException>(user == null);

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    await _asyncNotificationClient.SendEmailAsync(user.Email, _templateOptions.PasswordChanged);
                    model.PasswordChanged = true;
                    _telemetry?.TrackEvent(AiTracking.Events.ChangedPassword, HttpContext.ToTrackingMetadata());
                }
                else
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(GetErrorKey(error.Code), error.Description);
                    }
                }
            }

            return View(model);
        }

        public string GetErrorKey(string code)
        {
            return code switch
            {
                "PasswordRequiresNonAlphanumeric" or "PasswordRequiresDigit" or "PasswordRequiresUpper" => nameof(ChangePasswordViewModel.NewPassword),
                "PasswordMismatch" => nameof(ChangePasswordViewModel.CurrentPassword),
                _ => string.Empty,
            };
        }
    }
}
