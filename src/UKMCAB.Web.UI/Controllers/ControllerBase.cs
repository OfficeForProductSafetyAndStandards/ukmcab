namespace UKMCAB.Web.UI.Controllers
{
    using System.Security.Claims;
    using UKMCAB.Core.Security;
    using UKMCAB.Core.Services.Users;
    using UKMCAB.Data.Models.Users;

    public abstract class ControllerBase : Controller
    {
        protected readonly IUserService _userService;

        public ControllerBase(IUserService userService)
        {
            _userService = userService;
        }

        public UserAccount CurrentUser
        {
            get
            {
                return _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result ?? throw new InvalidOperationException();
            }
        }

        public string UserRoleId
        {
            get
            {
                return Roles.List.First(r => r.Label != null && r.Label.Equals(CurrentUser.Role, StringComparison.CurrentCultureIgnoreCase)).Id;
            }
        }
    }
}