using UKMCAB.Core.Services.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Search.RequestToUnarchiveCAB;

namespace UKMCAB.Web.UI.Areas.Search.Controllers;
[Area("search"), Route("search/request-to-unarchive/")]
public class RequestToUnarchiveCABController : Controller
{
    private readonly ICABAdminService _cabAdminService;

    public static class Routes
    {
        public const string RequestUnarchive = "cab.request.unarchive";
    }

    public RequestToUnarchiveCABController(ICABAdminService cabAdminService)
    {
        _cabAdminService = cabAdminService;
    }
    
    [HttpGet("{cabId}", Name = Routes.RequestUnarchive)]
    public async Task<IActionResult> IndexAsync(Guid cabId)
    {
        var vm = await BuildViewModelAsync(cabId);
        return View(vm);
    }

    [HttpPost("{cabId}", Name = Routes.RequestUnarchive)]
    public async Task<IActionResult> Index(Guid cabId, RequestToUnarchiveCABViewModel vm)
    {
        ModelState.Remove(nameof(vm.CABName));
        if (!ModelState.IsValid)
        {
            await BuildViewModelAsync(cabId);
            return View(vm);
        }
        
        //todo:
    }
    
    private async Task<RequestToUnarchiveCABViewModel> BuildViewModelAsync(Guid cabId)
    {
        var document = await _cabAdminService.GetLatestDocumentAsync(cabId.ToString()) ??
                       throw new InvalidOperationException("CAB not found");
        RequestToUnarchiveCABViewModel vm =
            new RequestToUnarchiveCABViewModel(document.Name ?? throw new InvalidOperationException(), cabId)
            {
                Title = "Request to unarchive CAB"
            };
        return vm;
    }
}