using Microsoft.AspNetCore.Mvc;
using UKMCAB.Data.CosmosDb;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Web.UI.Models;
using UKMCAB.Web.UI.Models.ViewModels.Admin;

namespace UKMCAB.Web.UI.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly ICosmosDbService _cosmosDbService;

    private const int CABS_PER_PAGE = 20;

    public static class RouteIds
    {
        public const string List = "admin.cab.list";
        public const string Create = "admin.cab.create";
        public const string Edit = "admin.cab.edit";
        public const string Feedback = "admin.cab.feedback";
    }


    public AdminController(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    [Route("", Name = RouteIds.List)]
    public async Task<IActionResult> ListAsync(int? pageNumber)
    {
        var totalCABs = await _cosmosDbService.GetCABCountAsync();
        if (pageNumber == null || (pageNumber - 1) * CABS_PER_PAGE > totalCABs)
        {
            pageNumber = 1;
        }

        var cabs = await _cosmosDbService.GetPagedCABsAsync(pageNumber.Value, CABS_PER_PAGE);

        var viewModel = new CabListViewModel
        {
            
            CabListItems = cabs.Select(c => new CabListItemViewModel { Id = c.Id, Name = c.Name, Address = c.Address }).ToList(),
            Pagination = new PaginationViewModel
            {
                PageNumber = pageNumber.Value,
                TotalPages = totalCABs / CABS_PER_PAGE
            }
        };
        if (totalCABs % CABS_PER_PAGE != 0)
        {
            viewModel.Pagination.TotalPages++;
        }
        
        return await Task.FromResult(View(viewModel));
    }


    [Route("edit/{id}", Name = RouteIds.Edit)]
    [Route("create", Name = RouteIds.Create)]
    public async Task<IActionResult> CreateEditAsync(string? id)
    {
        var editUrlTemplate = Url.RouteUrl(RouteIds.Feedback) 
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

    [Route("feedback", Name = RouteIds.Feedback)]
    public IActionResult Feedback()
    {
        return View();
    }
}