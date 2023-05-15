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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _authPassword;

        public UserController(UserManager<UKMCABUser> userManager, IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _authPassword = config["BasicAuthPassword"];
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTestUser testUser)
        {
            if(!_webHostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            Guard.IsTrue(_webHostEnvironment.IsDevelopment(), "UserController api is not enabled");

            if (!testUser.ApiPassword.Equals(_authPassword))
            {
                return Unauthorized();
            }

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
            if (!_webHostEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            Guard.IsTrue(_webHostEnvironment.IsDevelopment(), "UserController api is not enabled");

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
