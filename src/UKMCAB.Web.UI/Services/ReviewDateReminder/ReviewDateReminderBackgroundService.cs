using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Areas.Search.Controllers;

namespace UKMCAB.Web.UI.Services.ReviewDateReminder
{
    public class ReviewDateReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReviewDateReminderBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly IAppHost _appHost;
        private readonly LinkGenerator _linkGenerator;
        private readonly TelemetryClient _telemetryClient;
        private readonly Timer _timer;
        private DateTime _nextRunTime;
        private readonly IDistCache _distCache;

        public ReviewDateReminderBackgroundService(
            ILogger<ReviewDateReminderBackgroundService> logger, 
            TelemetryClient telemetryClient, 
            IServiceScopeFactory scopeFactory, 
            IAsyncNotificationClient notificationClient, 
            IOptions<CoreEmailTemplateOptions> templateOptions, 
            LinkGenerator linkGenerator, 
            IAppHost appHost, 
            IDistCache distCache)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _scopeFactory = scopeFactory;
            _notificationClient = notificationClient;
            _templateOptions = templateOptions.Value;
            _linkGenerator = linkGenerator;
            _appHost = appHost;
            _distCache = distCache;

            _distCache.InitialiseAsync();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var lockName = StringExt.Keyify(nameof(ReviewDateReminderBackgroundService), nameof(ExecuteAsync));
            var lockOwner = LockOwner.Create();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_nextRunTime == default || _nextRunTime.Date < DateTime.Today)
                {
                    _nextRunTime = DateTime.Today.AddHours(5);
                }

                var delay = (int)(_nextRunTime - DateTime.Now).TotalMilliseconds;
                delay = Math.Max(delay, 0);
                await Task.Delay(delay, stoppingToken);
                var gotLock = false;

                try
                {
                    gotLock = await _distCache.LockTakeAsync(lockName, lockOwner, TimeSpan.FromMinutes(1));
                    if (gotLock)
                    {
                        await CheckAndSendReviewDateReminder();
                    }
                }
                catch (Exception ex)
                {
                    _telemetryClient.TrackException(ex);
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    if (gotLock)
                    {
                        await _distCache.LockReleaseAsync(lockName, lockOwner);
                    }                    
                    _nextRunTime = _nextRunTime.AddDays(1);
                }
            }
        }
        private async Task CheckAndSendReviewDateReminder()
        {
            var scope = _scopeFactory.CreateScope();
            var _cabRepository = scope.ServiceProvider.GetRequiredService<ICABRepository>();

            var publishedCABs = await _cabRepository.Query<Document>(d => d.StatusValue == Status.Published);
            var publishedCABsWithDueReviewDates = publishedCABs.Where(d => IsReviewReminderNeeded(d.RenewalDate) || d.DocumentLegislativeAreas.Any(la => IsReviewReminderNeeded(la.ReviewDate)));
            var noOfReminderSent = 0;

            foreach (var cab in publishedCABsWithDueReviewDates)
            {
                try
                {
                    noOfReminderSent = await SendReminderAndCountSent(noOfReminderSent, cab);
                }
                catch(Exception ex) 
                {
                    _telemetryClient.TrackException(ex);
                    _logger.LogError(ex, $"Error sending review date reminder for : {cab.Name}");
                }                
            }

            if (noOfReminderSent > 0)
            {
                _logger.LogInformation($"{nameof(ReviewDateReminderBackgroundService)} ran successfully at {DateTime.Now}. {noOfReminderSent} CAB review date reminders sent.");
            }
            else
            {
                _logger.LogInformation($"{nameof(ReviewDateReminderBackgroundService)} ran successfully at {DateTime.Now}");
            }
        }

        private async Task<int> SendReminderAndCountSent(int noOfReminderSent, Document cab)
        {
            var resourcePath = _linkGenerator.GetPathByRouteValues(CABProfileController.Routes.CabDetails, new { id = cab.CABId }) ?? string.Empty;
            var baseUrl = _appHost.GetBaseUri().ToString() ?? string.Empty;
            baseUrl = baseUrl.EndsWith("/") ? baseUrl.Substring(0, baseUrl.Length - 1) : baseUrl;
            var fullUrl = $"{baseUrl}{resourcePath}";
            var user = new User("", "UKMCAB", "Service", Roles.OPSS.Id, _templateOptions.ContactUsOPSSEmail);

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
                        await SendInternalNotificationForLAReviewDateReminderAsync(cab, la, user, fullUrl);
                        noOfReminderSent++;
                    }
                }
            }

            return noOfReminderSent;
        }

        private bool IsReviewReminderNeeded(DateTime? renewalDate)
        {
            if (renewalDate == null) return false;

            var currentDate = DateTime.UtcNow.Date;

            return renewalDate == currentDate || renewalDate == currentDate.AddMonths(1) || renewalDate == currentDate.AddMonths(2);
        }

        private async Task SendInternalNotificationForCABReviewDateReminderAsync(Document cab, User user, DateTime reviewDate, string url)
        {
            var scope = _scopeFactory.CreateScope();
            var _workflowTaskService = scope.ServiceProvider.GetRequiredService<IWorkflowTaskService>();

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
        private async Task SendInternalNotificationForLAReviewDateReminderAsync(Document cab, DocumentLegislativeArea LA, User user, string url)
        {
            var scope = _scopeFactory.CreateScope();
            var _workflowTaskService = scope.ServiceProvider.GetRequiredService<IWorkflowTaskService>();

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
                    $"The review date for the {LA.LegislativeAreaName} legislative area associated with this CAB is {LA.ReviewDate.ToStringBeisDateFormat()}. This is a reminder to review the legislative area and ensure that all information is relevant and up to date.",
                    user,
                    DateTime.Now,
                    null,
                    null,
                    false,
                    Guid.Parse(cab.CABId),
                    null
                    ));
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
