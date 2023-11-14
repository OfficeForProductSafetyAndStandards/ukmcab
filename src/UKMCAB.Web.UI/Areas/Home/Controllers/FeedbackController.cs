using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Web.UI.Models.ViewModels.Feedback;

namespace UKMCAB.Web.UI.Areas.Home.Controllers
{
    [Area("Home")]
    public class FeedbackController : Controller
    {
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly ILoggingService _loggingService;

        public FeedbackController(IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> templateOptions, ILoggingService loggingService)
        {
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
            _loggingService = loggingService;
        }

        [HttpGet]
        [Route("feedback-form/submit")]
        public IActionResult Submit(string? returnURL)
        {
            return View(new FeedbackFormViewModel
            {
                ReturnURL = returnURL ?? "/"
            });
        }


        [HttpPost]
        [Route("feedback-form/submit")]
        public async Task<IActionResult> Submit(FeedbackFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SubmitEmailAsync(model);
                return RedirectToAction("Success", new {returnURL = model.ReturnURL}); 
                // let exceptions propagate.  they'll be handled by UnexpectedExceptionHandlerMiddleware which will show the standard friendly error message+error code.
                // if you override this behaviour, then exceptions won't get logged by application insights and will be less visible.
            }

            return View(model);
        }

        private async Task SubmitEmailAsync(FeedbackFormViewModel model)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                {"return-url", model.ReturnURL ?? "/" },
                {"browser-type", Request.Headers.UserAgent.ToString()},
                {"date-time", DateTime.UtcNow.ToString("f")},
                {"what-were-you-doing" , model.WhatWereYouDoing},
                {"what-went-wrong" , model.WhatWentWrong},
                {"email-address" , model.Email},
            };
            await _notificationClient.SendEmailAsync(_templateOptions.FeedbackEmail, _templateOptions.FeedbackForm, personalisation);

        }

        [HttpPost]
        [Route("feedback-form/submit-js")]
        public async Task<IActionResult> SubmitJs(FeedbackFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                await SubmitEmailAsync(model); // let exceptions propagate. they'll be handled by UnexpectedExceptionHandlerMiddleware which will show the standard friendly error message+error code.
                return Json(new FeedbackFormResult
                {
                    Submitted = true,
                    ErrorMessage = string.Empty
                });
            }

            return Json(new FeedbackFormResult
            {
                Submitted = false,
                ErrorMessage = "There was a problem submitting your feedback, please try again later."
            });
        }

        internal class FeedbackFormResult
        {
            public bool Submitted { get; set; }
            public string ErrorMessage { get; set; }
        }

        [Route("feedback-form/success")]
        public IActionResult Success(string returnURL)
        {
            return View(new FeedbackSuccessViewModel
            {
                ReturnURL = returnURL
            });
        }
    }
}
