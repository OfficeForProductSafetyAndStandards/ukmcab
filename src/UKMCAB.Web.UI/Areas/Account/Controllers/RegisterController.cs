using Microsoft.Extensions.Options;
using Notify.Interfaces;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using UKMCAB.Core.Models.Account;
using UKMCAB.Core.Services.Account;
using UKMCAB.Identity.Stores.CosmosDB;
using UKMCAB.Web.UI.Models.ViewModels.Account;

namespace UKMCAB.Web.UI.Areas.Account.Controllers
{
    [Area("Account")]
    public class RegisterController : Controller
    {
        private readonly IRegisterService _registerService;
        private readonly IAsyncNotificationClient _asyncNotificationClient;
        private readonly UserManager<UKMCABUser> _userManager;
        private readonly TemplateOptions _templateOptions;

        public RegisterController(IRegisterService registerService, IAsyncNotificationClient asyncNotificationClient, IOptions<TemplateOptions> templateOptions, UserManager<UKMCABUser> userManager)
        {
            _registerService = registerService;
            _asyncNotificationClient = asyncNotificationClient;
            _userManager = userManager;
            _templateOptions = templateOptions.Value;
        }

        [Route("account/{controller}")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("account/{controller}/request/{role}")]
        public async Task<IActionResult> RegisterRequest(string role)
        {
            var model = new RegisterRequestViewModel
            {
                LegislativeAreaList = Constants.LegislativeAreas,
                Role = role
            };

            return View(model);
        }

        [HttpPost]
        [Route("account/{controller}/request/{role}")]
        public async Task<IActionResult> RegisterRequest(RegisterRequestViewModel model, string role)
        {
            var returnUrl = Url.Content("~/");
            model.LegislativeAreaList = Constants.LegislativeAreas;
            if (!Constants.Roles.UKASUser.Equals(role, StringComparison.InvariantCultureIgnoreCase))
            {
                if (model.LegislativeAreas == null || !model.LegislativeAreas.Any())
                {
                    ModelState.AddModelError("LegislativeAreas", "Please select at least one legislative area from the list.");
                }

                if (string.IsNullOrWhiteSpace(model.RequestReason))
                {
                    ModelState.AddModelError("RequestReason", "Please enter a reason for the request.");
                }

            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    var encodedPayload = _registerService.EncodeRegistrationDetails(new RegistrationDTO
                    {
                        UserRole = role,
                        Email = model.Email,
                        Password = model.Password,
                        Reason = model.RequestReason,
                        LegislativeAreas = model.LegislativeAreas
                    });
                    var callbackUrl = Url.Action("ConfirmEmail", "Register", new { payload = encodedPayload },Request.Scheme, Request.Host.Value);
                    var personalisation = new Dictionary<string, dynamic>
                    {
                        {"register_link", HtmlEncoder.Default.Encode(callbackUrl)}
                    };
                    var response = await _asyncNotificationClient.SendEmailAsync(model.Email, _templateOptions.Register, personalisation);

                    return RedirectToAction("RegisterConfirmation");
                }
                ModelState.AddModelError("Email", "This email address has already been registered on the system.");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Route("account/{controller}/register-confirmation")]
        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [Route("account/{controller}/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string payload)
        {
            var model = new ConfirmEmailViewModel();
            var registrationDetails = _registerService.DecodeRegistrationDetails(payload);
            var userDetails = await _userManager.FindByEmailAsync(registrationDetails.Email);
            if (userDetails == null)
            {
                var user = new UKMCABUser
                {
                    UserName = registrationDetails.Email,
                    Email = registrationDetails.Email,
                    Regulations = registrationDetails.LegislativeAreas,
                    RequestReason = registrationDetails.Reason,
                    EmailConfirmed = true,
                    RequestApproved = registrationDetails.UserRole == Constants.Roles.UKASUser
                };
                var result = await _userManager.CreateAsync(user, registrationDetails.Password);
                var roleResult = await _userManager.AddToRoleAsync(user, registrationDetails.UserRole);
                if (result.Succeeded && roleResult.Succeeded)
                {
                    if (!registrationDetails.UserRole.Equals(Constants.Roles.UKASUser))
                    {
                        var response = await _asyncNotificationClient.SendEmailAsync(registrationDetails.Email,
                            _templateOptions.RegisterConfirmation);
                    }
                    model.Message = registrationDetails.UserRole == Constants.Roles.UKASUser ? "You will now be able to login to your account." : "Your registration request will be reviewed and you will receive notification once approved.";
                }
                else
                {
                    model.Message =
                        "There has been a problem registering your account. Please try again or contact an administrator.";
                }
            }
            else
            {
                model.Message = "There has been a problem with your registration details. Please try registering again or contact an administrator.";
            }

            return View(model);
        }

    }
}
