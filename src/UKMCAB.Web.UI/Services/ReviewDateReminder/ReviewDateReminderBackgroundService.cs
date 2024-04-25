using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Web.UI.Areas.Search.Controllers;

namespace UKMCAB.Web.UI.Services.ReviewDateReminder
{
    public class ReviewDateReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReviewDateReminderBackgroundService> _logger;
        private readonly ICABRepository _cabRepository;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly IAppHost _appHost;
        private readonly LinkGenerator _linkGenerator;


        public ReviewDateReminderBackgroundService(ILogger<ReviewDateReminderBackgroundService> logger, ICABRepository cabRepository, IWorkflowTaskService workflowTaskService, IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> templateOptions, LinkGenerator linkGenerator, IAppHost appHost)
        {
            _logger = logger;
            _cabRepository = cabRepository;
            _workflowTaskService = workflowTaskService;
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
            _linkGenerator = linkGenerator;
            _appHost = appHost;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(10_000);
            await CheckAndSendReviewDateReminder(stoppingToken);            
        }

        private async Task CheckAndSendReviewDateReminder(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var publishedCABs = await _cabRepository.Query<Document>(d => (d.StatusValue == Status.Published));
                var noOfReminderSent = 0;
                foreach (var cab in publishedCABs)
                {
                    var resourcePath = _linkGenerator.GetPathByRouteValues(CABProfileController.Routes.CabDetails, new { id = cab.CABId }) ?? string.Empty;
                    var baseUrl = _appHost.GetBaseUri().ToString() ?? string.Empty;
                    baseUrl = baseUrl.EndsWith("/") ? baseUrl.Substring(0, baseUrl.Length - 1) : baseUrl;
                    var fullUrl = $"{baseUrl}{resourcePath}";
                    var user = new User("", "System", "Notification",Roles.OPSS.Id, _templateOptions.ContactUsOPSSEmail);

                    var cabReviewDate = cab.RenewalDate;

                    if (IsReviewReminderNeeded(cabReviewDate))
                    {
                        await SendInternalNotificationForCABReviewDateReminderAsync(cab, user, (DateTime)cabReviewDate!, fullUrl);
                        noOfReminderSent++;
                    }

                    var legislativeAreasWithReviewDate = cab.DocumentLegislativeAreas.Where(la => la.ReviewDate != null);
                    if (legislativeAreasWithReviewDate != null)
                    {
                        foreach (var la in legislativeAreasWithReviewDate)
                        {
                            if (IsReviewReminderNeeded(la.ReviewDate))
                            {
                                await SendInternalNotificationForLAReviewDateReminderAsync(cab, la, user, (DateTime)cabReviewDate!, fullUrl);
                                noOfReminderSent++;
                            }
                        }
                    }
                }
                _logger.LogInformation($"{nameof(ReviewDateReminderBackgroundService)} ran successfully at {DateTime.Now}");
                if(noOfReminderSent > 0)
                {
                    var verb = noOfReminderSent > 1 ? "were" : "was";
                    _logger.LogInformation($"{noOfReminderSent} CAB review date reminders {verb} sent at: {DateTime.Now}");
                }
                
                await Task.Delay(600_000, stoppingToken);
            }
        }
        private bool IsReviewReminderNeeded(DateTime? renewalDate)
        {
            if (renewalDate == null) return false;

            var currentDate = DateTime.UtcNow.Date;

            return renewalDate == currentDate || renewalDate == currentDate.AddMonths(1) || renewalDate == currentDate.AddMonths(2);
        }

        private async Task SendInternalNotificationForCABReviewDateReminderAsync(Document cab, User user, DateTime reviewDate, string url)
        {
            var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cab.Name },
                { "CABReviewDate", reviewDate.ToStringBeisDateFormat()},
                {"CABUrl", url},
            };

            await _notificationClient.SendEmailAsync(_templateOptions.ContactUsOPSSEmail,
                _templateOptions.NotificationCabReviewReminder, personalisation);

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.CABReviewDue,
                    user, 
                    user.RoleId, 
                    null,
                    DateTime.Now,
                    $"The review date for this CAB is {reviewDate.ToStringBeisDateFormat()}. This is a reminder to review the CAB and ensure that all information is relevant and up to date.",
                    user,
                    DateTime.Now,
                    null,
                    null,
                    false,
                    Guid.Parse(cab.CABId),
                    null
                    ));
        } 
        private async Task SendInternalNotificationForLAReviewDateReminderAsync(Document cab, DocumentLegislativeArea LA, User user, DateTime cabReviewDate, string url)
        {        

            var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cab.Name },
                {"CABUrl", url},
                {"LAName", LA.LegislativeAreaName},
                {"LAReviewDate", LA.ReviewDate.ToStringBeisDateFormat() }
            };

            await _notificationClient.SendEmailAsync(_templateOptions.ContactUsOPSSEmail,
                _templateOptions.NotificationLegislativeAreaReviewReminder, personalisation);

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.LAReviewDue,
                    user, 
                    user.RoleId, 
                    null,
                    DateTime.Now,
                    $"The review date for this CAB is {cabReviewDate.ToStringBeisDateFormat()}. This is a reminder to review the CAB and ensure that all information is relevant and up to date.\nThe review date for the {LA.LegislativeAreaName} legislative area associated with this CAB is {LA.ReviewDate.ToStringBeisDateFormat()}. This is a reminder to review the legislative area and ensure that all information is relevant and up to date.",
                    user,
                    DateTime.Now,
                    null,
                    null,
                    false,
                    Guid.Parse(cab.CABId),
                    null
                    ));
        }
    }
}
