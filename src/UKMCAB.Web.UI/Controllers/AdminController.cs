using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    public static class RouteIds
    {
        public const string List = "admin.cab.list";
        public const string Create = "admin.cab.create";
        public const string Create2 = "admin.cab.create2";
        public const string Edit = "admin.cab.edit";
    }


    [Route("", Name = RouteIds.List)]
    public async Task<IActionResult> ListAsync()
    {
        return await Task.FromResult(View());
    }


    [Route("edit/{id}", Name = RouteIds.Edit)]
    [Route("create", Name = RouteIds.Create)]
    public async Task<IActionResult> CreateEditAsync()
    {
        return await Task.FromResult(View());
    }

    [Route("create2", Name = RouteIds.Create2)]
    public async Task<IActionResult> CreateEdit2Async()
    {
        return await Task.FromResult(View());
    }


}