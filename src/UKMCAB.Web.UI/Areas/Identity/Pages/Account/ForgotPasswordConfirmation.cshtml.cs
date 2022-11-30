// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel, ILayoutModel
    {
        private readonly UserManager<UKMCABUser> _userManager;
        public ForgotPasswordConfirmation(UserManager<UKMCABUser> userManager)
        {
            _userManager = userManager;
        }
        public bool DisplayConfirmAccountLink { get; set; }

        public string ResetConfirmationUrl { get; set; }

        public string? Title => "Forgotten password confirmation";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            if (TempData.Keys.Any(k => k.Equals("Code")))
            {
                // TODO: this is for testing purposes only and should be removed once email sending is introduced
                DisplayConfirmAccountLink = true;
                var code = (string)TempData["code"];
                ResetConfirmationUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
            }
            return Page();
        }
    }
}
