
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using System.Diagnostics;
using System.Security.Claims;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.CosmosDb.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Search.Models;
using UKMCAB.Data.Search.Services;
using UKMCAB.Web.UI.Areas.Admin.Controllers;
using YamlDotNet.Core;

namespace UKMCAB.Web.UI.Services.ReviewDateReminder
{
    public class ReviewDateReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReviewDateReminderBackgroundService> _logger;
        private readonly ICABAdminService _cabAdminService;
        private readonly ICachedSearchService _cachedSearchService;
        private readonly ICABRepository _cabRepository;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;


        public ReviewDateReminderBackgroundService(ILogger<ReviewDateReminderBackgroundService> logger, ICABAdminService cabAdminService, ICachedSearchService cachedSearchService, ICABRepository cabRepository, IWorkflowTaskService workflowTaskService, IAsyncNotificationClient notificationClient, IOptions<CoreEmailTemplateOptions> templateOptions)
        {
            _logger = logger;
            _cabAdminService = cabAdminService;
            _cachedSearchService = cachedSearchService;
            _cabRepository = cabRepository;
            _workflowTaskService = workflowTaskService;
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var publishedCABs = await _cabRepository.Query<Document>(d => (d.StatusValue == Status.Published));

                Debug.WriteLine("Working behind the scenes...");
                _logger.LogInformation("START: Background servive now running at: {time}", DateTime.Now);
                foreach (var cab in publishedCABs)
                {
                    var reviewDate = cab.RenewalDate;
                    
                    if (reviewDate != null)

                    {
                        string? reminderTimeAhead = IsRenewalDateApproaching((DateTime)reviewDate);
                        if (reminderTimeAhead != null)
                        {
                            var userAccount = new UserAccount
                            {
                                Id = "",
                                FirstName = "OPSS",
                                Surname = "Admin",
                                Role = "opss",
                                EmailAddress = "opssadmin@opss.com" // Hard-code details???
                            };
                            _logger.LogInformation("{CAB} is due for renewal in {timeAhead}", cab.Name, reminderTimeAhead);
                            await SendInternalNotificationForCABRenewalDateReminderAsync(cab, userAccount, (DateTime)reviewDate);
                        }
                    }
                }
                _logger.LogInformation("END: Background servive stopped running at: {time}", DateTime.Now);
                Debug.WriteLine("Finished working behind the scenes...");
                await Task.Delay(60_000, stoppingToken);
            }
        }

        private string? IsRenewalDateApproaching(DateTime renewalDate)
        {
            var currentDate = DateTime.UtcNow.Date;
            var oneDayAhead = currentDate.AddDays(1);
            var oneMonthAhead = currentDate.AddMonths(1);
            var twoMonthsAhead = currentDate.AddMonths(2);

            if (renewalDate == oneDayAhead)
            {
                return nameof(oneDayAhead);
            }
            else if (renewalDate == oneMonthAhead)
            {
                return nameof(oneMonthAhead);
            }
            else if (renewalDate == twoMonthsAhead)
            { 
                return nameof(twoMonthsAhead); 
            }

            return null;
        }

        private async Task SendInternalNotificationForCABRenewalDateReminderAsync(Document cab, UserAccount userAccount,
           DateTime reviewDate)
        {
            var personalisation = new Dictionary<string, dynamic?>
            {
                { "CABName", cab.Name },
                { "CABReviewDate", cab.RenewalDate.ToStringBeisDateFormat() }
            };

            await _notificationClient.SendEmailAsync(_templateOptions.ApprovedBodiesEmail,
                _templateOptions.NotificationCabReviewReminder, personalisation);

            var user = new User(userAccount.Id, userAccount.FirstName, userAccount.Surname,
                userAccount.Role ?? throw new InvalidOperationException(),
                userAccount.EmailAddress ?? throw new InvalidOperationException());

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.ReviewCAB,
                    user, // Who should be submitter? System or opss
                    "opss", //Should it be hardcoded opss
                    null,
                    DateTime.Now,
                    $"The review date for this CAB is {reviewDate}. This is a reminder to review the CAB and ensure that all information is relevant and up to date.",
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
