using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Areas.Test.Model;

namespace UKMCAB.Web.UI.Areas.Test.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<UKMCABUser> _userManager;

        public UserController(UserManager<UKMCABUser> userManager, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTestUser testUser)
        {
            if (ModelState.IsValid)
            {
                var user = new UKMCABUser
                {
                    UserName = testUser.Email,
                    Email = testUser.Email,
                    EmailConfirmed = true,
                    RequestApproved = true,
                    FirstName = testUser.FirstName,
                    LastName = testUser.LastName
                };
                var result = await _userManager.CreateAsync(user, testUser.Password);
                var roleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.OPSSAdmin);
                return result.Succeeded && roleResult.Succeeded ? Ok(user.Id) : BadRequest();

            }

            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteTestUser testuser)
        {
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
