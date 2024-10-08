using System.Net;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Web.UI.Models.ViewModels.Footer;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class FooterController : Controller
    {
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;

        public static class Routes
        {
            public const string ContactUs = "footer.contact-us";
        }

        public FooterController(IAsyncNotificationClient asyncNotificationClient, IOptions<CoreEmailTemplateOptions> templateOptions)
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

        /*
        // TEMPORARILY HIDE Contact Us functionality - UKMCAB-1983 / hotfix 4.2.1
        */
        [Route("/contact-us", Name = Routes.ContactUs)]
        public IActionResult ContactUs(string? returnUrl)
        {
            returnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : default;

            return View(new ContactUsViewModel { 
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? null : WebUtility.UrlDecode(returnUrl), 
                Email = _templateOptions.ContactUsOPSSEmail
            });
        }

    }
}
