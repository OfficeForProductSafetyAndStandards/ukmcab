
using UKMCAB.Web.UI.Models.ViewModels.Cookies;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class CookiesController : Controller
    {
        [Route("/cookies-policy")]
        public IActionResult Cookies()
        {
            var model = new CookiesViewModel
            {
                Cookies = Request.Cookies.ContainsKey(Constants.AnalyticsOptInCookieName) ? Request.Cookies[Constants.AnalyticsOptInCookieName] : null
            };
            return View(model);
        }

        [Route("/cookies-policy")]
        [HttpPost]
        public IActionResult Cookies(CookiesViewModel model)
        {
            if (model.Cookies == "reject")
            {
                var cookiesToDelete = Request.Cookies.Keys.Where(c => c.StartsWith("ai_") || c.StartsWith("_ga"));
                foreach (var cookie in cookiesToDelete)
                {
                    Response.Cookies.Delete(cookie);
                }
            }
            Response.Cookies.Append(Constants.AnalyticsOptInCookieName, model.Cookies, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) });
            if (!string.IsNullOrWhiteSpace(model.ReturnURL) && Url.IsLocalUrl(model.ReturnURL))
            {
                return Redirect(model.ReturnURL);
            }

            TempData[Constants.TempCookieChangeKey] = true;
            return RedirectToAction("Cookies");
        }
    }
}
