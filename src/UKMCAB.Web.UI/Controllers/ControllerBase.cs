namespace UKMCAB.Web.UI.Controllers
{
    using Humanizer;
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

        public UserAccount CurrentUser => _userService.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)).Result ?? throw new InvalidOperationException();

        //TODO: Why do we need to re-fetch the user object every time we want to check its role?
        public string UserRoleId => CurrentUser.Role ?? throw new InvalidOperationException();
    }
}