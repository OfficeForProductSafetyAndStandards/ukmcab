using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin")]
    //[Area("admin"), Authorize]
    public class ServiceManagementController : Controller
    {
        public static class Routes
        {
            public const string ServiceManagement = "service.management";
        }

        [HttpGet("admin/service-management", Name = "service.management")]
        public IActionResult ServiceManagement()
        {
            return View();
        }
    }
}
