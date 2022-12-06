using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UKMCAB.Identity.Stores.CosmosDB;

namespace UKMCAB.Web.UI.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = Constants.Roles.OPSSAdmin)]
    public class PendingAccountRequestsModel : PageModel, ILayoutModel
    {
        private readonly UserManager<UKMCABUser> _userManager;
        public PendingAccountRequestsModel(UserManager<UKMCABUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UKMCABUser> PendingUsers { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(Constants.Roles.OGDUser);
            
            PendingUsers = usersInRole.Where(u => u.EmailConfirmed && !u.RequestApproved).ToList();
            return Page();
        }

        public string? Title => "Pending account requests";
    }
}
