using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Areas.Test.Model;

namespace UKMCAB.Web.UI.Areas.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly string _authPassword;

        public UserController(UserManager<UKMCABUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _authPassword = config["BasicAuthPassword"];
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTestUser testUser)
        {
            if (!testUser.ApiPassword.Equals(_authPassword))
            {
                return Unauthorized();
            }
            var user = new UKMCABUser
            {
                UserName = testUser.Email,
                Email = testUser.Email,
                EmailConfirmed = true,
                RequestApproved = true
            };
            var result = await _userManager.CreateAsync(user, testUser.Password);
            var roleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.OPSSAdmin);
            return result.Succeeded && roleResult.Succeeded ? Ok(user.Id) : BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteTestUser testuser)
        {
            if (!testuser.ApiPassword.Equals(_authPassword))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByEmailAsync(testuser.Email);
            if (user != null)
            {
                var roleResult = await _userManager.RemoveFromRoleAsync(user, Constants.Roles.OPSSAdmin);
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded && roleResult.Succeeded ? Ok() : BadRequest();
            }

            return Ok();
        }
    }
}
