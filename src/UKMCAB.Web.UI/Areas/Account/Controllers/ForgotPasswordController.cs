using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using System.Text.Encodings.Web;
using UKMCAB.Common.Exceptions;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account")]
    public class ForgotPasswordController : Controller
    {
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly TemplateOptions _templateOptions;

        public ForgotPasswordController(IAsyncNotificationClient asyncNotificationClient, UserManager<UKMCABUser> userManager, IOptions<TemplateOptions> templateOptions)
        {
            _asyncNotificationClient = asyncNotificationClient;
            _userManager = userManager;
            _templateOptions = templateOptions.Value;
        }

        [Route("account/forgot-password")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("account/forgot-password")]
        public async Task<IActionResult> Index(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("Confirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("Reset", "ForgotPassword", new { code = code }, Request.Scheme, Request.GetOriginalHostFromHeaders());

                var personalisation = new Dictionary<string, dynamic>
                {
                    {"reset_link", HtmlEncoder.Default.Encode(callbackUrl)}
                };

                await _asyncNotificationClient.SendEmailAsync(model.Email, _templateOptions.ResetPassword, personalisation);

                return RedirectToAction("Confirmation");
            }

            return View(model);
        }

        [Route("account/forgot-password/confirmation")]
        public IActionResult Confirmation()
        {
            return View();
        }

        [Route("account/forgot-password/reset")]
        public IActionResult Reset(string code)
        {
            Guard.IsFalse<NotFoundException>(code == null);
            return View(new ResetPasswordViewModel
            {
                Code = code
            });
        }

        [HttpPost]
        [Route("account/forgot-password/reset")]
        public async Task<IActionResult> Reset(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
                var result = await _userManager.ResetPasswordAsync(user, code, model.Password);
                if (result.Succeeded)
                {
                    await _asyncNotificationClient.SendEmailAsync(model.Email, _templateOptions.PasswordReset);

                    return RedirectToAction("ResetPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [Route("account/forgot-password/reset-confirmation")]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
