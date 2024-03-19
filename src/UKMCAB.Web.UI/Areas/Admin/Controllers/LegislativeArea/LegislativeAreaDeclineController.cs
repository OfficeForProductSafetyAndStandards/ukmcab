using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;

[Area("admin"), Route("admin/cab/{id}/legislative-area-decline/"), Authorize]
public class LegislativeAreaDeclineController
{
    private readonly ICABAdminService _cabAdminService;
    private readonly ILegislativeAreaService _legislativeAreaService;
    private readonly IUserService _userService;
    
    public static class Routes
    {
        public const string LegislativeAreaDecline = "legislative.area.decline";
    }

    public LegislativeAreaDeclineController(ICABAdminService cabAdminService,
        ILegislativeAreaService legislativeAreaService,
        IUserService userService)
    {
        _cabAdminService = cabAdminService;
        _legislativeAreaService = legislativeAreaService;
        _userService = userService;
    }

    [HttpGet("decline", Name = Routes.LegislativeAreaDecline)]
    public async Task<IActionResult> AdditionalInformationAsync(Guid id)
    {
        return View("~/Areas/Admin/views/CAB/LegislativeArea/AdditionalInformation.cshtml", vm);
    }
}