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
        private readonly TemplateOptions _templateOptions;

        public ManageController(UserManager<UKMCABUser> userManager, SignInManager<UKMCABUser> signInManager, IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _asyncNotificationClient = asyncNotificationClient;
            _templateOptions = templateOptions.Value;
        }

        [Route("account/manage/change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            Guard.IsFalse<PermissionDeniedException>(user == null);

            return View();
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
                    TempData["Email"] = user.Email;
                    return RedirectToAction("PasswordChanged");
                }
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [Route("account/manage/password-changed")]
        public async Task<IActionResult> PasswordChanged()
        {
            var user = await _userManager.GetUserAsync(User);
            Guard.IsTrue<PermissionDeniedException>(TempData["Email"] is string emailValidation && user != null && user.Email == emailValidation);
            return View();
        }
    }
}
