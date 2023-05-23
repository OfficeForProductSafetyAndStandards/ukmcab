
namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class CookiesController : Controller
    {
        [Route("/cookies-policy")]
        public IActionResult Cookies()
        {
            var model = new BasicPageModel
            {
                Title = Constants.PageTitle.CookiesPolicy
            };
            return View(model);
        }

        [Route("/cookies-policy")]
        [HttpPost]
        public IActionResult Cookies(string returnURL, string cookies)
        {
            if (cookies == "reject")
            {
                var cookiesToDelete = Request.Cookies.Keys.Where(c => c.StartsWith("ai_") || c.StartsWith("_ga"));
                foreach (var cookie in cookiesToDelete)
                {
                    Response.Cookies.Delete(cookie);
                }
            }
            Response.Cookies.Append(Constants.AnalyticsOptInCookieName, cookies, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) });
            if (!string.IsNullOrWhiteSpace(returnURL))
            {
                return Redirect(returnURL);
            }
            var model = new BasicPageModel
            {
                Title = Constants.PageTitle.CookiesPolicy
            };
            return View(model);
        }
    }
}
