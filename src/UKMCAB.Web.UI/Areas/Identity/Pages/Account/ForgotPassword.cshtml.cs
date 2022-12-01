// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel, ILayoutModel
    {
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly TemplateOptions _templateOptions;

        public ForgotPasswordModel(UserManager<UKMCABUser> userManager, IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions)
        {
            _userManager = userManager;
            _asyncNotificationClient = asyncNotificationClient;
            _templateOptions = templateOptions.Value;
        }

        public string? Title => "Forgotten password";

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [RegularExpression("^.+(?:@ukas\\.com|@.+\\.gov\\.uk)$", ErrorMessage = "Only ukas.com or .gov.uk email addresses are valid")]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
                var personalisation = new Dictionary<string, dynamic>
                {
                    {"reset_link", HtmlEncoder.Default.Encode(callbackUrl)}
                };
                var response = await _asyncNotificationClient.SendEmailAsync(Input.Email, _templateOptions.ResetPassword, personalisation);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

    }
}
