using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Data.CosmosDb.Models;

namespace UKMCAB.Web.UI.Controllers
{
    [ApiController]
    [Route("api/{controller}")]
    public class CABsController : ControllerBase
    {
        private readonly ICosmosDbService _cosmosDbService;

        public CABsController(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cabs = await _cosmosDbService.GetPagedCABsAsync(1, 10);
            return Ok(cabs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var cab = await _cosmosDbService.GetByIdAsync(id);
            if (cab != null)
            {
                return Ok(cab);
            }

            return NotFound();
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
