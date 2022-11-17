// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel, ILayoutModel
    {
        private readonly UserManager<UKMCABUser> _userManager;

        public ConfirmEmailModel(UserManager<UKMCABUser> userManager)
        {
            _userManager = userManager;
        }

        public string? Title => "Email confirmation";


        public string ConfirmationTitle { get; set; }
        public string ConfirmationBody { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                ConfirmationTitle = "Thank you for confirming your email.";
                var isOGD = await _userManager.IsInRoleAsync(user, Constants.Roles.OGDUser);
                if (isOGD)
                {
                    ConfirmationBody =
                        "Your registration request will be reviewed and you will receive notification once approved.";
                }
                else
                {
                    user.RequestApproved = true;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        ConfirmationBody = "You will now be able to login to your account.";
                    }
                    else
                    {
                        ConfirmationTitle = "There has been an error";
                        ConfirmationBody = "Please try again or contact and administrator.";
                    }
                }
            }
            else
            {
                ConfirmationTitle = "There has been an error";
                ConfirmationBody = "Please try again or contact and administrator.";
            }
            return Page();
        }
    }
}
