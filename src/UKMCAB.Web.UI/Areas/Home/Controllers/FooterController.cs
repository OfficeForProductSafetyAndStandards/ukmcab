using System.Net;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Web.UI.Models.ViewModels.Footer;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class FooterController : Controller
    {
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly TemplateOptions _templateOptions;
        public FooterController(IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions)
        {
            _asyncNotificationClient = asyncNotificationClient;
            _templateOptions = templateOptions.Value;
        }


        [Route("/privacy-notice")]
        public IActionResult Privacy()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.PrivacyNotice
            };
            return View(model);
        }

        [Route("/cookies-policy")]
        public IActionResult Cookies()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.CookiesPolicy
            };
            return View(model);
        }        
        
        [Route("/accessibility-statement")]
        public IActionResult AccessibilityStatement()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.AccessibilityStatement
            };
            return View(model);
        }        
        
        [Route("/terms-and-conditions")]
        public IActionResult TermsAndConditions()
        {
            var model = new BasicPageModel()
            {
                Title = Constants.PageTitle.TermsAndConditions
            };
            return View(model);
        }


        [Route("/contact-us")]
        public IActionResult ContactUs(string? returnUrl)
        {
            return View(new ContactUsViewModel{ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? WebUtility.UrlDecode("/") : WebUtility.UrlDecode(returnUrl) });
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
                    return RedirectToAction("ContactUsConfirmation", new {returnUrl = WebUtility.UrlEncode(model.ReturnUrl)});
                }
                catch
                {
                    ModelState.AddModelError("Email", "Sorry, we were unable to send you message. Please check your email address or try again later.");
                }

            }

            return View(model);
        }

        [Route("/contact-us-confirmation")]
        public IActionResult ContactUsConfirmation(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = WebUtility.UrlEncode("/");
            }
            return View(new ContactUsConfirmationViewModel{ReturnUrl = returnUrl});
        }
    }
}
