using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Web.UI.Models.ViewModels.Home;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class HomeController : Controller
    {
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly TemplateOptions _templateOptions;

        public HomeController(IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions)
        {
            _asyncNotificationClient = asyncNotificationClient;
            _templateOptions = templateOptions.Value;
        }

        [Route("/")]
        [Route("home")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("/help")]
        public IActionResult Help()
        {
            return View();
        }

        [Route("/contact-us")]
        public IActionResult ContactUs()
        {
            return View(new ContactUsViewModel());
        }

        [HttpPost]
        [Route("/contact-us")]
        public async Task<IActionResult> ContactUs(ContactUsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var personalisation = new Dictionary<string, dynamic>
                {
                    {"name", model.Name},
                    {"email", model.Email},
                    {"subject", model.Subject},
                    {"message", model.Message}
                };

                try
                {
                    await _asyncNotificationClient.SendEmailAsync(_templateOptions.ContactUsOPSSEmail, _templateOptions.ContactUsOPSS, personalisation);
                    await _asyncNotificationClient.SendEmailAsync(model.Email, _templateOptions.ContactUsUser, personalisation);
                    return RedirectToAction("ContactUsConfirmation");
                }
                catch
                {
                    ModelState.AddModelError("Email", "Sorry, we were unable to send you message. Please check your email address or try again later.");
                }

            }
            return View(model);
        }

        [Route("/contact-us-confirmation")]
        public IActionResult ContactUsConfirmation()
        {
            return View();
        }

    }
}
