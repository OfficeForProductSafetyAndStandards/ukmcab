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

        [Route("account/manage/pending-requests")]
        [Authorize(Roles = Constants.Roles.OPSSAdmin)]
        public async Task<IActionResult> PendingRequests()
        {
            var ogdUsers = await _userManager.GetUsersInRoleAsync(Constants.Roles.OGDUser);
            var opssUsers = await _userManager.GetUsersInRoleAsync(Constants.Roles.OPSSAdmin);

            var model = new PendingAccountsViewModel
            {
                PendingUsers = ogdUsers.Union(opssUsers).Where(u => u.EmailConfirmed && !u.RequestApproved).ToList()
            };

            return View(model);
        }

        [Route("account/manage/request-review/{id}")]
        [Authorize(Roles = Constants.Roles.OPSSAdmin)]
        public async Task<IActionResult> RequestReview(string id)
        {
            Guard.IsFalse<NotFoundException>(string.IsNullOrWhiteSpace(id));

            var user = await _userManager.FindByIdAsync(id);

            Guard.IsTrue<NotFoundException>(user != null);
            Rule.IsTrue(user.EmailConfirmed, "Email address has not been confirmed");

            return View(new RequestReviewViewModel { UserForReview = user});
        }

        [HttpPost]
        [Route("account/manage/request-review/{id}")]
        [Authorize(Roles = Constants.Roles.OPSSAdmin)]
        public async Task<IActionResult> RequestReview(RequestReviewViewModel model, string id, string Command)
        {
            var user = await _userManager.FindByIdAsync(id);

            Guard.IsTrue<NotFoundException>(user != null);
            Rule.IsTrue(user.EmailConfirmed, "Email address has not been confirmed");

            if (Command == "Approve")
            {
                user.RequestApproved = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _asyncNotificationClient.SendEmailAsync(user.Email, _templateOptions.RegistrationApproved, new Dictionary<string, dynamic>{{"login_link", Url.Action("Login", "Home", null,Request.Scheme, Request.GetOriginalHostFromHeaders())}});
                    return RedirectToAction("PendingRequests");
                }

                foreach (var identityError in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, identityError.Description);
                }
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    await _asyncNotificationClient.SendEmailAsync(user.Email, _templateOptions.RegistrationRejected, new Dictionary<string, dynamic>{{"reject_reason", string.IsNullOrWhiteSpace(model.RejectionReason) ? "None given" : model.RejectionReason } });
                    return RedirectToAction("PendingRequests");
                }

                foreach (var identityError in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, identityError.Description);
                }
            }

            return View("RequestReview", model);
        }
    }
}
