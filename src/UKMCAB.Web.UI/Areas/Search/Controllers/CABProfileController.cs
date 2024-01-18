using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Security.Claims;
using System.Xml;
using Microsoft.Extensions.Options;
using Notify.Interfaces;
using UKMCAB.Common.Exceptions;
using UKMCAB.Common.Extensions;
using UKMCAB.Core.Domain.Workflow;
using UKMCAB.Core.EmailTemplateOptions;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Core.Services.Workflow;
using UKMCAB.Data;
using UKMCAB.Data.CosmosDb.Services.CachedCAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.Users;
using UKMCAB.Data.Storage;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Web.UI.Areas.Home.Controllers;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Web.UI.Models.ViewModels.Search;
using UKMCAB.Web.UI.Models.ViewModels.Shared;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Web.UI.Areas.Search.Controllers
{
    [Area("search")]
    public class CABProfileController : Controller
    {
        private readonly ICachedPublishedCABService _cachedPublishedCabService;
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly TelemetryClient _telemetryClient;
        private readonly IFeedService _feedService;
        private readonly IUserService _userService;
        private readonly CoreEmailTemplateOptions _templateOptions;
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly IWorkflowTaskService _workflowTaskService;
        private readonly IEditLockService _editLockService;

        public static class Routes
        {
            public const string CabDetails = "cab.detail";
            public const string CabDraftProfile = "cab.profile.draft";
            public const string CabProfileHistoryDetails = "cab.profile.history-details";
            public const string TrackInboundLinkCabDetails = "cab.details.inbound-email-link";
            public const string CabFeed = "cab.feed";
        }

        public CABProfileController(ICachedPublishedCABService cachedPublishedCabService,
            ICABAdminService cabAdminService, IFileStorage fileStorage, TelemetryClient telemetryClient,
            IFeedService feedService, IUserService userService, IOptions<CoreEmailTemplateOptions> templateOptions,
            IAsyncNotificationClient notificationClient, IWorkflowTaskService workflowTaskService,
            IEditLockService editLockService)
        {
            _cachedPublishedCabService = cachedPublishedCabService;
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _telemetryClient = telemetryClient;
            _feedService = feedService;
            _userService = userService;
            _templateOptions = templateOptions.Value;
            _notificationClient = notificationClient;
            _workflowTaskService = workflowTaskService;
            _editLockService = editLockService;
        }

        [HttpGet("~/__subscriptions/__inbound/cab/{id}", Name = Routes.TrackInboundLinkCabDetails)]
        public IActionResult TrackInboundLinkCabDetails(string id)
        {
            _telemetryClient.TrackEvent(AiTracking.Events.CabViewedViaSubscriptionsEmail,
                HttpContext.ToTrackingMetadata());
            return RedirectToRoute(Routes.CabDetails, new { id });
        }

        [HttpGet("search/cab-profile/{id}", Name = Routes.CabDetails)]
        public async Task<IActionResult> Index(string id, string? returnUrl, string? unlockCab, int pagenumber = 1)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            if (cabDocument != null && !id.Equals(cabDocument.URLSlug))
            {
                return RedirectToActionPermanent("Index", new { id = cabDocument.URLSlug, returnUrl });
            }

            if (cabDocument == null || (cabDocument.StatusValue == Status.Archived && !User.Identity.IsAuthenticated))
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(unlockCab))
            {
                await _editLockService.RemoveEditLockForCabAsync(unlockCab);
            }

            var userAccount = User.Identity is { IsAuthenticated: true }
                ? await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value)
                : null;
            var unarchiveNotifications = await _workflowTaskService.GetByCabIdAndTaskTypeAsync(
                cabDocument.CABId.ToGuid()!.Value,
                new List<TaskType> { TaskType.RequestToUnarchiveForDraft, TaskType.RequestToUnarchiveForPublish });
            var requireApproval = userAccount != null && !string.Equals(userAccount.Role, Roles.OPSS.Label,
                StringComparison.CurrentCultureIgnoreCase);
            var cab = await GetCabProfileViewModel(
                cabDocument,
                returnUrl,
                userAccount != null,
                requireApproval && (!unarchiveNotifications.Any() || unarchiveNotifications.All(t => t.Completed)),
                pagenumber);
            cab = await GetUnarchiveRequestInformation(cab);

            _telemetryClient.TrackEvent(AiTracking.Events.CabViewed, HttpContext.ToTrackingMetadata(new()
            {
                [AiTracking.Metadata.CabId] = id,
                [AiTracking.Metadata.CabName] = cab.Name
            }));

            return View(cab);
        }

        [HttpGet("profile/draft/{id:guid}", Name = Routes.CabDraftProfile), Authorize]
        public async Task<IActionResult> DraftAsync(string id, int pagenumber = 1)
        {
            var cabDocument = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var cab = await GetCabProfileViewModel(cabDocument, null, true, false, pagenumber);
                return View("Index", cab);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("search/cab-profile-feed/{id}", Name = Routes.CabFeed)]
        public async Task<IActionResult> Feed(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);
            if (cabDocument == null)
            {
                throw new NotFoundException($"The CAB with the following CAB url count not be found: {id}");
            }

            var feed = _feedService.GetSyndicationFeed(cabDocument.Name, Request, cabDocument, Url);

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                Indent = true,
                ConformanceLevel = ConformanceLevel.Document
            };

            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    feed.GetAtom10Formatter().WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }

                stream.Position = 0;
                var content = new StreamReader(stream).ReadToEnd();
                return Content(content, "application/atom+xml;charset=utf-8");
            }
        }

        private async Task<CABProfileViewModel> GetCabProfileViewModel(Document cabDocument, string? returnUrl,
            bool loggedIn = false, bool showRequestToUnarchive = false, int pagenumber = 1)
        {
            var isArchived = cabDocument.StatusValue == Status.Archived;
            var auditLogOrdered = cabDocument.AuditLog.OrderBy(a => a.DateTime).ToList();

            var isUnarchivedRequest =
                auditLogOrdered.Last().Action == AuditCABActions.UnarchiveRequest; //todo should be notifications
            var isPublished = cabDocument.StatusValue == Status.Published;
            var archiveAudit = isArchived ? auditLogOrdered.Last(al => al.Action == AuditCABActions.Archived) : null;
            var publishedAudit = auditLogOrdered.LastOrDefault(al => al.Action == AuditCABActions.Published);

            var fullHistory = await _cachedPublishedCabService.FindAllDocumentsByCABIdAsync(cabDocument.CABId);
            var hasDraft = fullHistory.Any(d => d.StatusValue == Status.Draft);

            var history = new AuditLogHistoryViewModel(fullHistory, pagenumber, loggedIn);

            var cab = new CABProfileViewModel
            {
                IsArchived = isArchived,
                IsUnarchivedRequest = isUnarchivedRequest,
                ShowRequestToUnarchive = showRequestToUnarchive,
                IsPublished = isPublished,
                HasDraft = hasDraft,
                ArchivedBy = isArchived && archiveAudit != null ? archiveAudit.UserName : string.Empty,
                ArchivedDate = isArchived && archiveAudit != null
                    ? archiveAudit.DateTime.ToStringBeisFormat()
                    : string.Empty,
                ArchiveReason = isArchived && archiveAudit != null ? archiveAudit.Comment : string.Empty,
                AuditLogHistory = history,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : WebUtility.UrlDecode(returnUrl),
                IsOPSSUser = User.IsInRole(Roles.OPSS.Id),
                CABId = cabDocument.CABId,
                CABUrl = cabDocument.URLSlug,
                PublishedDate = publishedAudit?.DateTime ?? null,
                LastModifiedDate = cabDocument.LastUpdatedDate,
                Name = cabDocument.Name,
                AppointmentDate = cabDocument.AppointmentDate,
                ReviewDate = cabDocument.RenewalDate,
                UKASReferenceNumber = cabDocument.UKASReference,
                Address = cabDocument.GetFormattedAddress(),
                Website = cabDocument.Website,
                Email = cabDocument.Email,
                Phone = cabDocument.Phone,
                PointOfContactName = cabDocument.PointOfContactName ?? string.Empty,
                PointOfContactEmail = cabDocument.PointOfContactEmail ?? string.Empty,
                PointOfContactPhone = cabDocument.PointOfContactPhone ?? string.Empty,
                IsPointOfContactPublicDisplay = cabDocument.IsPointOfContactPublicDisplay,
                BodyNumber = cabDocument.CABNumber,
                CabNumberVisibility = cabDocument.CabNumberVisibility,
                BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                Status = cabDocument.Status,
                SubStatus = cabDocument.SubStatus.GetEnumDescription(),
                StatusCssStyle = CssClassUtils.CabStatusStyle(cabDocument.StatusValue),
                LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                ProductSchedules = new CABDocumentsViewModel
                {
                    Id = "product-schedules",
                    Title = "Product schedules",
                    CABId = cabDocument.CABId,
                    Documents = cabDocument.Schedules?.Select(s => new FileUpload
                    {
                        Label = s.Label ?? s.FileName,
                        FileName = s.FileName,
                        BlobName = s.BlobName,
                        LegislativeArea = s.LegislativeArea
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Schedules
                },
                SupportingDocuments = new CABDocumentsViewModel
                {
                    Id = "supporting-documents",
                    Title = "Supporting documents",
                    CABId = cabDocument.CABId,
                    Documents = cabDocument.Documents?.Select(s => new FileUpload
                    {
                        Label = s.Label ?? s.FileName,
                        FileName = s.FileName,
                        BlobName = s.BlobName,
                        Category = s.Category
                    }).ToList() ?? new List<FileUpload>(),
                    DocumentType = DataConstants.Storage.Documents
                },
                GovernmentUserNotes = new UserNoteListViewModel(new Guid(cabDocument.id),
                    cabDocument.GovernmentUserNotes, pagenumber),
                FeedLinksViewModel = new FeedLinksViewModel
                {
                    FeedUrl = Url.RouteUrl(Routes.CabFeed, new { id = cabDocument.CABId }),
                    EmailUrl = Url.RouteUrl(
                        Subscriptions.Controllers.SubscriptionsController.Routes.Step0RequestCabSubscription,
                        new { id = cabDocument.CABId }),
                    CABName = cabDocument.Name
                }
            };

            ShareUtils.AddDetails(HttpContext, cab.FeedLinksViewModel);

            return cab;
        }

        private async Task<CABProfileViewModel> GetUnarchiveRequestInformation(CABProfileViewModel profileViewModel)
        {
            var tasks = await _workflowTaskService.GetByCabIdAsync(Guid.Parse(profileViewModel.CABId));
            var task = tasks.FirstOrDefault(t =>
                t.TaskType is TaskType.RequestToUnarchiveForDraft or TaskType.RequestToUnarchiveForPublish &&
                !t.Completed);

            int? summaryBreak = task?.Body.Length > 60
                ? task.Body.Substring(0, 60).LastIndexOf(" ", StringComparison.Ordinal)
                : null;
            profileViewModel.UnarchiverFirstAndLastName = task?.Submitter.FirstAndLastName;
            profileViewModel.UnarchiverUserGroup = task?.Submitter.UserGroup;
            profileViewModel.UnarchiveReasonSummary =
                summaryBreak.HasValue ? task?.Body.Substring(0, summaryBreak.Value) : null;
            profileViewModel.UnarchiveReason = task?.Body;
            profileViewModel.UnarchiveTaskType = task?.TaskType;
            return profileViewModel;
        }

        #region ArchiveCAB
        
        [HttpGet, Route("search/archive-cab/{cabUrl}"), Authorize]
        public async Task<IActionResult> ArchiveCAB(string cabUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(cabUrl);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {cabUrl}");
            if (cabDocument.StatusValue != Status.Published)
            {
                return RedirectToAction("Index", new { url = cabUrl });
            }

            var draft = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(cabDocument.CABId);
            return View(new ArchiveCABViewModel
            {
                CABId = cabDocument.CABId,
                Name = cabDocument.Name,
                HasDraft = draft != null
            });
        }
        
        [HttpPost, Route("search/archive-cab/{cabUrl}"), Authorize]
        public async Task<IActionResult> ArchiveCAB(string cabUrl, ArchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(cabUrl);

            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {cabUrl}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                //Get draft before archiving deletes it
                var draft = await _cachedPublishedCabService.FindDraftDocumentByCABIdAsync(cabDocument.CABId);
                await _cabAdminService.ArchiveDocumentAsync(userAccount, cabDocument.CABId, model.ArchiveInternalReason,
                    model.ArchivePublicReason);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = cabDocument.CABId,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));

                if (draft != null)
                {
                    await SendDraftCabDeletedNotification(userAccount, cabDocument,
                        draft.AuditLog.First(al => al.Action == AuditCABActions.Created).UserId);
                }

                return RedirectToAction("Index", new { id = cabDocument.URLSlug });
            }

            model.Name = cabDocument.Name ?? string.Empty;
            return View(model);
        }

        /// <summary>
        /// Sends a notification when archived and an associated draft is deleted.
        /// </summary>
        /// <param name="archiverAccount"></param>
        /// <param name="cabDocument"></param>
        /// <param name="draftUserId"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task SendDraftCabDeletedNotification(UserAccount archiverAccount, Document cabDocument,
            string draftUserId)
        {
            var draftCreator = await _userService.GetAsync(draftUserId);
            if (draftCreator == null)
            {
                return;
            }

            await SendEmailForDeleteDraftAsync(cabDocument.Name!, draftCreator.EmailAddress!);

            var archiverRoleId = Roles.RoleId(archiverAccount.Role) ?? throw new InvalidOperationException();

            var archiver = new User(archiverAccount.Id, archiverAccount.FirstName, archiverAccount.Surname,
                archiverRoleId, archiverAccount.EmailAddress ?? throw new InvalidOperationException());

            var cabCreatorRoleId = Roles.RoleId(draftCreator.Role) ?? throw new InvalidOperationException();

            var assignee = new User(draftCreator.Id, draftCreator.FirstName, draftCreator.Surname,
                cabCreatorRoleId, draftCreator.EmailAddress ?? throw new InvalidOperationException());

            await _workflowTaskService.CreateAsync(
                new WorkflowTask(
                    TaskType.DraftCabDeletedFromArchiving,
                    archiver,
                    assignee.RoleId,
                    assignee,
                    DateTime.Now,
                    $"The draft record for {cabDocument.Name} has been deleted because the CAB profile was archived. Contact UKMCAB support if you need the draft record to be added to the service again.",
                    archiver,
                    DateTime.Now,
                    false,
                    null,
                    true,
                    Guid.Parse(cabDocument.CABId)));
        }

        private async Task SendEmailForDeleteDraftAsync(string cabName, string receiverEmailAddress)
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "CABName", cabName },
                {
                    "ContactSupportURL", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(FooterController.Routes.ContactUs))
                },
                {
                    "NotificationsURL", UriHelper.GetAbsoluteUriFromRequestAndPath(HttpContext.Request,
                        Url.RouteUrl(Admin.Controllers.NotificationController.Routes.Notifications))
                }
            };

            await _notificationClient.SendEmailAsync(receiverEmailAddress,
                _templateOptions.NotificationDraftCabDeletedFromArchiving, personalisation);
        }

        #endregion
        
        [HttpGet, Route("search/unarchive-cab/{id}"), Authorize]
        public async Task<IActionResult> UnarchiveCAB(string id, string? returnUrl)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            Guard.IsTrue(cabDocument != null, $"No published document found for CAB URL: {id}");
            if (cabDocument.StatusValue != Status.Archived)
            {
                return RedirectToAction("Index", new { url = id, returnUrl });
            }

            return View(new UnarchiveCABViewModel
            {
                CABId = id,
                CABName = cabDocument.Name,
                ReturnURL = returnUrl
            });
        }

        [HttpPost]
        [Route("search/unarchive-cab/{id}")]
        public async Task<IActionResult> UnarchiveCAB(string id, UnarchiveCABViewModel model)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABURLOrGuidAsync(id);
            Guard.IsTrue(cabDocument != null, $"No archived document found for CAB URL: {id}");
            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UnarchiveDocumentAsync(userAccount, cabDocument.CABId,
                    model.UnarchiveInternalReason, model.UnarchivePublicReason, false);
                _telemetryClient.TrackEvent(AiTracking.Events.CabArchived, HttpContext.ToTrackingMetadata(new()
                {
                    [AiTracking.Metadata.CabId] = id,
                    [AiTracking.Metadata.CabName] = cabDocument.Name
                }));
                return RedirectToAction("Summary", "CAB", new { area = "Admin", id = cabDocument.CABId });
            }

            model.CABName = cabDocument.Name;

            return View(model);
        }


        /// <summary>
        /// CAB data API used by the Email Subscriptions Core
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("~/__api/subscriptions/core/cab/{id}")]
        public async Task<IActionResult> GetCabAsync(string id)
        {
            var cabDocument = await _cachedPublishedCabService.FindPublishedDocumentByCABIdAsync(id);

            if (cabDocument != null)
            {
                var auditLogOrdered = cabDocument.AuditLog.OrderBy(a => a.DateTime).ToList();
                var publishedAudit = auditLogOrdered.Last(al => al.Action == AuditCABActions.Published);
                var cab = new SubscriptionsCoreCabModel
                {
                    CABId = cabDocument.CABId,
                    PublishedDate = publishedAudit.DateTime,
                    LastModifiedDate = cabDocument.LastUpdatedDate,
                    Name = cabDocument.Name,
                    UKASReferenceNumber = string.Empty,
                    Address = StringExt.Join(", ", cabDocument.AddressLine1, cabDocument.AddressLine2,
                        cabDocument.TownCity, cabDocument.Postcode, cabDocument.Country),
                    Website = cabDocument.Website,
                    Email = cabDocument.Email,
                    Phone = cabDocument.Phone,
                    BodyNumber = cabDocument.CABNumber,
                    BodyTypes = cabDocument.BodyTypes ?? new List<string>(),
                    RegisteredOfficeLocation = cabDocument.RegisteredOfficeLocation,
                    RegisteredTestLocations = cabDocument.TestingLocations ?? new List<string>(),
                    LegislativeAreas = cabDocument.LegislativeAreas ?? new List<string>(),
                    ProductSchedules = cabDocument.Schedules?.Select(pdf => new SubscriptionsCoreCabFileModel
                    {
                        BlobName = pdf.BlobName,
                        FileName = pdf.FileName
                    }).ToList() ?? new List<SubscriptionsCoreCabFileModel>(),
                };
                return Json(cab);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("search/cab-schedule-download/{id}")]
        public async Task<IActionResult> Download(string id, string file, string filetype)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{filetype}/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType, file);
            }

            return Ok("File does not exist");
        }

        [HttpGet("search/cab-schedule-view/{id}")]
        public async Task<IActionResult> View(string id, string file, string filetype)
        {
            var fileStream = await _fileStorage.DownloadBlobStream($"{id}/{filetype}/{file}");
            if (fileStream != null)
            {
                return File(fileStream.FileStream, fileStream.ContentType);
            }

            return Ok("File does not exist");
        }

        [HttpGet("search/cab/history-details", Name = Routes.CabProfileHistoryDetails)]
        public async Task<IActionResult> CABHistoryDetails(string date, string time, string username, string userId,
            string userGroup, string auditAction, string internalComment, string? publicComment, string? returnUrl,
            bool isUserInputComment)
        {
            var auditHistoryItemViewModel = new AuditHistoryItemViewModel
            {
                Date = date,
                Time = time,
                Username = username,
                UserId = userId,
                Usergroup = userGroup,
                Action = auditAction,
                InternalComment = internalComment,
                PublicComment = publicComment ?? Constants.NotProvided,
                ReturnUrl = WebUtility.UrlDecode(returnUrl),
                IsUserInputComment = isUserInputComment
            };

            return View(auditHistoryItemViewModel);
        }
    }
}