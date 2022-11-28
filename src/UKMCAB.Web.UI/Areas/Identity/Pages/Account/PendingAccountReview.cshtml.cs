using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using UKMCAB.Common.Exceptions;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account
{
    [Authorize(Roles = Constants.Roles.Administrator)]
    public class PendingAccountReviewModel : PageModel, ILayoutModel
    {
        private readonly UserManager<UKMCABUser> _userManager;
        public PendingAccountReviewModel(UserManager<UKMCABUser> userManager)
        {
            _userManager = userManager;
        }

        public UKMCABUser? UserForReview { get; set; }

        [BindProperty] 
        public InputModel? Input { get; set; }

        public class InputModel
        {
            public string? UserId { get; set; }

            [Display(Name = "What is the reason for rejection?")]
            public string? RejectionReason { get; set; }
        }

        public string? Title => "Pending account review";

        public async Task<ActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            Guard.IsTrue<NotFoundException>(user != null);
            Rule.IsTrue(user.EmailConfirmed, "Email address has not been confirmed");

            UserForReview = user;
            Input = new InputModel
            {
                UserId = user.Id
            };

            return Page();
        }

        public async Task<ActionResult> OnPostApproveAsync()
        {
            var user = await _userManager.FindByIdAsync(Input.UserId);

            Guard.IsTrue<NotFoundException>(user != null);
            Rule.IsTrue(user.EmailConfirmed, "Email address has not been confirmed");

            user.RequestApproved = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // TODO: send email
                return RedirectToPage("PendingAccountRequests");
            }

            foreach (var identityError in result.Errors)
            {
                ModelState.AddModelError(string.Empty, identityError.Description);
            }
            return Page();
        }

        public async Task<ActionResult> OnPostRejectAsync()
        {
            var user = await _userManager.FindByIdAsync(Input.UserId);

            Guard.IsTrue<NotFoundException>(user != null);
            Rule.IsTrue(user.EmailConfirmed, "Email address has not been confirmed");

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                // TODO: send email
                return RedirectToPage("PendingAccountRequests");
            }

            foreach (var identityError in result.Errors)
            {
                ModelState.AddModelError(string.Empty, identityError.Description);
            }
            return Page();
        }
    }
}
