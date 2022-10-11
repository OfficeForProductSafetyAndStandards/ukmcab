using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Web.UI.Controllers
{
    [ApiController]
    [Route("api/cabs")]
    public class CABsController : ControllerBase
    {
        private readonly ICosmosDbService _cosmosDbService;

        public CABsController(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCAB(CAB cab)
        {
            var resouceId = await _cosmosDbService.CreateAsync(cab);
            if (!string.IsNullOrEmpty(resouceId))
            {
                return Ok(resouceId);
            }

            return BadRequest();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCAB(CAB cab)
        {
            if (!string.IsNullOrEmpty(cab.Id))
            {
                var success = await _cosmosDbService.UpdateAsync(cab);
                return success ? Ok(success) : BadRequest();
            }

            return BadRequest();
        }
    }
}
