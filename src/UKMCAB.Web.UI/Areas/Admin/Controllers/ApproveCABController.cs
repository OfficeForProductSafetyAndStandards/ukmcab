using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers;

[Area("admin"), Authorize]
public class ApproveCABController: Controller
{
    private readonly ICABAdminService _cabAdminService;

    public ApproveCABController(ICABAdminService cabAdminService)
    {
        _cabAdminService = cabAdminService;
    }

    public static class Routes
    {
        public const string Approve = "cab.approve";
    }

    [HttpGet("/cab/approve/{id}", Name = Routes.Approve)]
    public async Task<IActionResult> Approve(string id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id) ?? throw new InvalidOperationException("CAB not found");
        var model = new ApproveCABViewModel("Approve CAB", document.CABId,
            document.Name ?? throw new InvalidOperationException(), string.Empty);
        
        return View("~/Areas/Admin/Views/CAB/Approve.cshtml",model);
    }
    
    [HttpPost("/cab/approve/{id}")]
    public async Task<IActionResult> ApprovePost(string id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id) ?? throw new InvalidOperationException("CAB not found");
        var model = new ApproveCABViewModel("Approve CAB", document.CABId,
            document.Name ?? throw new InvalidOperationException(), string.Empty);
        if (!ModelState.IsValid)
        {
            return View("~/Areas/Admin/Views/CAB/Approve.cshtml",model);
        }

        document.CABNumber = model.CABNumber;
        _cabAdminService.PublishDocumentAsync()
         
        return View("~/Areas/Admin/Views/CAB/Approve.cshtml",model);
    }
}