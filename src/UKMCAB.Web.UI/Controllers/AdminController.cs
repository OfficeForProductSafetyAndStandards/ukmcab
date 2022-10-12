using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Web.UI.Models;

namespace UKMCAB.Web.UI.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly ICosmosDbService _cosmosDbService;

    public static class RouteIds
    {
        public const string List = "admin.cab.list";
        public const string Create = "admin.cab.create";
        public const string Edit = "admin.cab.edit";
    }


    public AdminController(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    [Route("", Name = RouteIds.List)]
    public async Task<IActionResult> ListAsync()
    {
        return await Task.FromResult(View());
    }


    [Route("edit/{id}", Name = RouteIds.Edit)]
    [Route("create", Name = RouteIds.Create)]
    public async Task<IActionResult> CreateEditAsync(string? id)
    {
        var editUrlTemplate = Url.RouteUrl(RouteIds.Edit, new { id = "guid" }) 
            ?? throw new Exception("Route not found for the edit url template");
        if (id != null)
        {
            var cab = await _cosmosDbService.GetByIdAsync(id) ?? throw new Exception("CAB not found");
            return View(new CreateEditCabViewModel { Data = cab, EditUrlTemplate = editUrlTemplate });
        }
        else
        {
            return View(new CreateEditCabViewModel { EditUrlTemplate = editUrlTemplate });
        }
    }

}