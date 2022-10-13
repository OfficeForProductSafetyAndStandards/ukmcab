using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

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
    public async Task<IActionResult> ListAsync(int? pageNumber)
    {
        if (pageNumber == null)
        {
            pageNumber = 1;
        }

        var cabs = await _cosmosDbService.GetAllAsync(pageNumber.Value, 10);
        var viewModel = new CabListViewModel
        {
            CabListItems = cabs.Select(c => new CabListItemViewModel { Id = c.Id, Name = c.Name, Address = c.Address }).ToList(),
            PageNumber = pageNumber.Value,
        };
        

        return await Task.FromResult(View(viewModel));
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