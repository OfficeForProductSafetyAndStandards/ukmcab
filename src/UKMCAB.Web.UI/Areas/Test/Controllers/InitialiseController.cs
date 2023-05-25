using Microsoft.AspNetCore.Authorization;
using UKMCAB.Data;

namespace UKMCAB.Web.UI.Areas.Test.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Constants.Roles.OPSSAdmin}")]
    [ApiController]
    public class InitialiseController : Controller
    {
        private readonly IInitialiseDataService _initialiseDataService;

        public InitialiseController(IInitialiseDataService initialiseDataService)
        {
            _initialiseDataService = initialiseDataService;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var cutOffDate = new DateTime(2023, 5, 27);
            try
            {
                if (DateTime.UtcNow < cutOffDate)
                {
                    await _initialiseDataService.InitialiseAsync(true);
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Initialisation failed.{e.Message}. {e.StackTrace}");
            }

            return Ok();
        }
    }
}
