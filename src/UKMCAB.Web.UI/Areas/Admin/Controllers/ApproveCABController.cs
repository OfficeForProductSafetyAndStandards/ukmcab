using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;

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

    [HttpGet("admin/cab/approve/{id}", Name = Routes.Approve)]
    public async Task<IActionResult> Approve(string id)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(id) ?? throw new InvalidOperationException("CAB not found");
        (string CABId, string CABName) model = (document.CABId ?? throw new InvalidOperationException(), document.Name ?? throw new InvalidOperationException());
        return View("~/Areas/Admin/Views/CAB/Approve.cshtml",model);
    }
}