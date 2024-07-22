using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule;
using UKMCAB.Web.UI.Services;
using Document = UKMCAB.Data.Models.Document;
using UKMCAB.Web.UI.Helpers;
using UKMCAB.Core.Security;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadController : UI.Controllers.ControllerBase
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IFileUploadUtils _fileUploadUtils;
        private readonly ILegislativeAreaService _legislativeAreaService;
        private readonly IDistCache _distCache;

        public static class Routes
        {
            public const string SchedulesList = "file-upload.schedules-list";
            public const string SchedulesListRemove = "file-upload.schedules-list-remove";
            public const string SchedulesListRemoveWithOption = "file-upload.schedules-list-remove-with-option";
        }

        public FileUploadController(
            ICABAdminService cabAdminService,
            IFileStorage fileStorage,
            IUserService userService,
            IFileUploadUtils fileUploadUtils,
            ILegislativeAreaService legislativeAreaService,
            IDistCache distCache) : base(userService)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _fileUploadUtils = fileUploadUtils;
            _legislativeAreaService = legislativeAreaService;
            _distCache = distCache;
        }

        [HttpGet("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // Pre-populate model for edit
            if (latestVersion.Schedules is { Count: >= SchedulesOptions.MaxFileCount })
            {
                return RedirectToAction("SchedulesList", fromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var model = new FileUploadViewModel
            {
                Title = SchedulesOptions.UploadTitle,                
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // Pre-populate model for edit
            latestVersion.Schedules ??= new List<FileUpload>();
            if (latestVersion.Schedules.Count >= SchedulesOptions.MaxFileCount)
            {
                return RedirectToAction("SchedulesList",
                    model.IsFromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var allowableNoOfFiles = SchedulesOptions.MaxFileCount - latestVersion.Schedules.Count;

            if (model.Files != null && model.Files.Count <= allowableNoOfFiles)
            {
                foreach (var file in model.Files)
                {
                    var contentType =
                        _fileUploadUtils.GetContentType(file, SchedulesOptions.AcceptedFileExtensionsContentTypes);

                    if (ValidateUploadFile(file, contentType, latestVersion.Schedules))
                    {
                        var result = await _fileStorage.UploadCABFile(latestVersion.CABId, file.FileName, file.FileName,
                            DataConstants.Storage.Schedules,
                            file.OpenReadStream(), contentType);
                        latestVersion.Schedules.Add(result);

                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                    }
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("SchedulesList",
                        model.IsFromSummary
                            ? new { id = latestVersion.CABId, fromSummary = "true" }
                            : new { id = latestVersion.CABId });
                }
            }
            else if (model.Files != null && model.Files.Count > allowableNoOfFiles)
            {
                ModelState.AddModelError("File",
                    $"Max upload is 35. You can only upload {allowableNoOfFiles} file(s) more.");
            }
            else if (model.Files == null && model.File == null)
            {
                ModelState.AddModelError("File",
                    $"Select a {SchedulesOptions.AcceptedFileTypes} file 10 megabytes or less.");
            }

            model.Title = SchedulesOptions.UploadTitle;           
            model.CABId = id;
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/save-as-draft/{id}")]
        public async Task<IActionResult> SaveAsDraft(string id)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var userAccount =
                await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion, false);
            TempData[Constants.TempDraftKey] =
                $"Draft record saved for {latestVersion.Name} <br>CAB number {latestVersion.CABNumber}";
            return RedirectToAction("CABManagement", "CabManagement",
                new { Area = "admin", unlockCab = latestVersion.CABId });
        }

        [HttpGet]
        [Route("admin/cab/schedules-list/{id}", Name = Routes.SchedulesList)]
        public async Task<IActionResult> SchedulesList(string id, bool fromSummary, string? returnUrl, string? SelectedScheduleId,
            ProductScheduleActionMessageEnum? actionType)
        {   
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            string? successBannerTitle = default;            

            if (latestDocument == null) // Implies no document or document archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            await _cabAdminService.FilterCabContentsByLaIfPendingOgdApproval(latestDocument, UserRoleId);

            var uploadedFileViewModels = latestDocument.Schedules?.Select(s => new FileViewModel
            {
                FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                LegislativeArea = s.LegislativeArea?.Trim(), Archived = s.Archived, Id = s.Id
            }).ToList() ?? new List<FileViewModel>();


            if (!string.IsNullOrWhiteSpace(SelectedScheduleId))
            {
                var selectedSchedule = uploadedFileViewModels.Where(n => n.Id == Guid.Parse(SelectedScheduleId)).FirstOrDefault();

                if (selectedSchedule != null)
                {
                    selectedSchedule.IsSelected = true;
                }
            }

            if(actionType.HasValue)
            {
                successBannerTitle = AlertMessagesUtils.ProductScheduleActionMessages[actionType.Value];
            }

            // Pre-populate model for edit
            return View(new ScheduleFileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                ArchivedFiles = uploadedFileViewModels.Where(n => n.Archived == true).ToList(),
                ActiveFiles = uploadedFileViewModels.Where(n => n.Archived is null or false).ToList(),
                CABId = id,
                IsFromSummary = fromSummary,
                ReturnUrl = returnUrl,
                SuccessBannerTitle = successBannerTitle,                
                DocumentStatus = latestDocument.StatusValue,
                LegislativeAreas = GetDocumentAreaDistinctLegislativeAreas(latestDocument),
                ShowArchiveAction = !await _cabAdminService.IsSingleDraftDocAsync(Guid.Parse(id))
            }); 
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}", Name = Routes.SchedulesList)]
        public async Task<IActionResult> SchedulesList(string id, string submitType, ScheduleFileListViewModel model, 
            bool fromSummary)
        {
            var cabDocuments = await _cabAdminService.FindAllDocumentsByCABIdAsync(id.ToString());
            var latestDocument = _cabAdminService.GetLatestDocumentFromDocuments(cabDocuments);

            if (latestDocument == null) // Implies no document or document archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var schedules = latestDocument.Schedules ?? new List<FileUpload>();

            if (submitType is Constants.SubmitType.Cancel)
            {
                if (fromSummary)
                {
                    return RedirectToAction("Summary", "CAB", new { id, revealEditActions = true });
                }
                else
                {
                    return RedirectToAction("CABManagement", "CABManagement", new { Area = "admin", unlockCab = id });
                }
            }

            if((submitType is Constants.SubmitType.Remove || submitType is Constants.SubmitType.Archive || submitType is Constants.SubmitType.ReplaceFile || submitType is Constants.SubmitType.UseFileAgain) && string.IsNullOrEmpty(model.SelectedScheduleId))
            {                
                ModelState.AddModelError("SelectedScheduleId", "Select a schedule");
            }
            
            if (submitType is Constants.SubmitType.RemoveArchived && string.IsNullOrEmpty(model.SelectedArchivedScheduleId))
            {
                ModelState.AddModelError("SelectedArchivedScheduleId", "Select an archived schedule");
            }

            if (submitType == Constants.SubmitType.Continue || submitType == Constants.SubmitType.Save)
            {
                AddLegislativeLabelAndFileModelStateErrors(model);
            }

            if (submitType is Constants.SubmitType.Continue)
            {
                AddLegislativeSelectionModelStateErrors(model);
            }

            if (ModelState.IsValid)
            {
                if (submitType is Constants.SubmitType.Remove)
                {
                    var filesInViewModel = model.ActiveFiles ?? new List<FileViewModel>();
                    var schedule = schedules.Where(n => n.Id == Guid.Parse(model.SelectedScheduleId)).First();

                    if (schedule != null)
                    {
                        // only one document and draft mode then remove the schedule else give user an option to remove or archive the schedule
                        if (cabDocuments.Count == 1 && cabDocuments.First().StatusValue == Status.Draft)
                        {
                            latestDocument.Schedules.Remove(schedule);

                            var userAccount =
                                await _userService.GetAsync(User.Claims
                                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                    .Value);
                            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

                            return RedirectToAction("SchedulesList", new { id, actionType = ProductScheduleActionMessageEnum.ProductScheduleRemoved, fromSummary });

                        }
                        else
                        {   
                            // check if no legislative area assigned to schedule or legislative area have more than 1 product schedule
                            var redirectToRemoveSchedule = string.IsNullOrWhiteSpace(schedule.LegislativeArea) || schedules.Where(n => n.LegislativeArea == schedule.LegislativeArea).Count() > 1;

                            if (redirectToRemoveSchedule)
                            {
                                return RedirectToRoute(Routes.SchedulesListRemove, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Remove, fromSummary });                                
                            }
                            else
                            {
                                return RedirectToRoute(Routes.SchedulesListRemoveWithOption, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Remove, fromSummary });

                            }
                        }
                    }                                       
                }
                else if (submitType is Constants.SubmitType.Archive)
                {
                    var filesInViewModel = model.ActiveFiles ?? new List<FileViewModel>();
                    var schedule = schedules.Where(n => n.Id == Guid.Parse(model.SelectedScheduleId)).First();

                    if (schedule != null)
                    {
                        var legislativearea = schedule.LegislativeArea;
                        var redirectToRemoveSchedule = string.IsNullOrWhiteSpace(schedule.LegislativeArea) || schedules.Where(n => n.LegislativeArea == schedule.LegislativeArea).Count() > 1;

                        if (redirectToRemoveSchedule)
                        {
                            return RedirectToRoute(Routes.SchedulesListRemove, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Archive });
                        }
                        else
                        {
                            return RedirectToRoute(Routes.SchedulesListRemoveWithOption, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Archive });
                        }
                    }
                }
                else if (submitType is Constants.SubmitType.RemoveArchived)
                {
                    var filesInViewModel = model.ActiveFiles ?? new List<FileViewModel>();
                    var schedule = schedules.Where(n => n.Id == Guid.Parse(model.SelectedArchivedScheduleId)).First();

                    if (schedule != null)
                    {
                        // only one document and draft mode then remove the schedule else give user an option to remove or archive the schedule
                        if (cabDocuments.Count == 1 && cabDocuments.First().StatusValue == Status.Draft)
                        {
                            schedules.Remove(schedule);

                            var userAccount =
                                await _userService.GetAsync(User.Claims
                                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                    .Value);
                            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
                        }                       
                        else
                        {                               
                            // check if no legislative area assigned to schedule or legislative area have more than 1 product schedule
                            var redirectToRemoveSchedule = string.IsNullOrWhiteSpace(schedule.LegislativeArea) || schedules.Where(n => n.LegislativeArea == schedule.LegislativeArea).Count() > 1;

                            if (redirectToRemoveSchedule)
                            {
                                return RedirectToRoute(Routes.SchedulesListRemove, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Remove });
                            }
                            else
                            {
                                return RedirectToRoute(Routes.SchedulesListRemoveWithOption, new { id, scheduleId = schedule.Id, actionType = RemoveActionEnum.Remove });
                            }
                        }
                    }
                }
                else if (submitType is Constants.SubmitType.UseFileAgain)
                {
                    var schedule = schedules.Where(n => n.Id == Guid.Parse(model.SelectedScheduleId)).First() ?? throw new InvalidOperationException("Can't find selected schedule"); 

                    var newSchedule = new FileUpload()
                    { 
                        Id = Guid.NewGuid(),
                        Label = schedule.Label,
                        FileName = schedule.FileName,
                        BlobName = schedule.BlobName,
                        Archived = null,
                        Category = null,
                        LegislativeArea = string.Empty,
                        UploadDateTime = DateTime.UtcNow,
                    };

                    schedules.Add(newSchedule);

                    var userAccount =  await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);

                    return RedirectToAction("SchedulesList", "FileUpload", new { id, fromSummary, model.SelectedScheduleId, actionType = ProductScheduleActionMessageEnum.ProductScheduleFileUsedAgain });

                }
                else if (submitType is Constants.SubmitType.ReplaceFile)
                {
                    var schedule = schedules.Where(n => n.Id == Guid.Parse(model.SelectedScheduleId)).First() ?? throw new InvalidOperationException("Can't find selected schedule");
                    return RedirectToAction("SchedulesReplaceFile", "FileUploadManagement", new { id, scheduleId = schedule.Id, fromSummary });
                }
                else if (submitType == Constants.SubmitType.UploadAnother)
                {
                    return model.IsFromSummary
                        ? RedirectToAction("SchedulesUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId, fromSummary = true })
                        : RedirectToAction("SchedulesUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId });
                }
                else
                {
                    if (UpdateFiles(latestDocument, model.ActiveFiles))
                    {
                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
                    }

                    if (submitType == Constants.SubmitType.Continue)
                    {
                        return model.IsFromSummary
                            ? RedirectToAction("Summary", "CAB",
                                new { Area = "admin", id = latestDocument.CABId, revealEditActions = true })
                            : RedirectToAction("DocumentsUpload", "FileUpload",
                                new { Area = "admin", id = latestDocument.CABId });
                    }
                    else if (submitType == Constants.SubmitType.Save)
                    {
                        return SaveDraft(latestDocument);
                    }
                }
            }

            var uploadedFileViewModels = schedules.Select(s => new FileViewModel
            {
                FileName = s.FileName,
                UploadDateTime = s.UploadDateTime,
                Label = s.Label,
                LegislativeArea = s.LegislativeArea?.Trim(),
                Archived = s.Archived,
                Id = s.Id
            }).ToList() ?? new List<FileViewModel>();

            model.ArchivedFiles = uploadedFileViewModels.Where(n => n.Archived == true).ToList();
            model.DocumentStatus = latestDocument.StatusValue;
            model.LegislativeAreas = GetDocumentAreaDistinctLegislativeAreas(latestDocument);

            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            // Pre-populate model for edit
            latestVersion.Documents ??= new List<FileUpload>();

            var model = new FileUploadViewModel()
            {
                Title = DocumentsOptions.UploadTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel
                {
                    FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, Category = s.Category
                }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            };
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            latestVersion.Documents ??= new List<FileUpload>();

            if (model.Files != null)
            {
                foreach (var file in model.Files)
                {
                    var contentType =
                        _fileUploadUtils.GetContentType(file, DocumentsOptions.AcceptedFileExtensionsContentTypes);

                    if (ValidateUploadFile(file, contentType, latestVersion.Documents))
                    {
                        var result = await _fileStorage.UploadCABFile(latestVersion.CABId, file.FileName, file.FileName,
                            DataConstants.Storage.Documents,
                            file.OpenReadStream(), contentType);
                        latestVersion.Documents.Add(result);
                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                    }
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("DocumentsList",
                        model.IsFromSummary
                            ? new { id = latestVersion.CABId, fromSummary = "true" }
                            : new { id = latestVersion.CABId });
                }
            }
            else if (model.Files == null && model.File == null)
            {
                ModelState.AddModelError("File",
                    $"Select a {DocumentsOptions.AcceptedFileTypes} file 10 megabytes or less.");
            }

            model.Title = DocumentsOptions.UploadTitle;
            model.UploadedFiles =
                latestVersion.Documents?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                        Category = s.Category, Id = s.Id, Archived = s.Archived
                    })
                    .ToList() ?? new List<FileViewModel>();
            model.CABId = id;
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, bool fromSummary, string? returnUrl, string? indexOfSelectedFile,
            string? fromAction)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var uploadedFileViewModels = new List<FileViewModel>();

            if (fromAction == nameof(FileUploadManagementController.DocumentsReplaceFile) ||
                fromAction == nameof(FileUploadManagementController.DocumentsUseFileAgain))
            {
                if (fromAction == nameof(FileUploadManagementController.DocumentsReplaceFile) &&
                    latestDocument.Documents != null &&
                    int.TryParse(indexOfSelectedFile, out var indexOfFileToReplace) &&
                    indexOfFileToReplace < latestDocument.Documents.Count)
                {
                    UpdateFileVMIsReplacedPropertyAndLoad(latestDocument.Documents, uploadedFileViewModels,
                        indexOfFileToReplace);
                }

                if (fromAction == nameof(FileUploadManagementController.DocumentsUseFileAgain) &&
                    latestDocument.Documents != null &&
                    int.TryParse(indexOfSelectedFile, out var fileToUseAgainIndex) &&
                    fileToUseAgainIndex < latestDocument.Documents.Count)
                {
                    uploadedFileViewModels = latestDocument.Documents?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                        Category = s.Category, Archived = s.Archived, Id = s.Id
                    }).ToList() ?? new List<FileViewModel>();

                    var selectedViewModel = latestDocument.Documents[fileToUseAgainIndex];
                    AddSelectedVMToUploadedFileViewModels(uploadedFileViewModels, selectedViewModel);
                }

                UpdateShowBannerAndSuccessBanner(uploadedFileViewModels, out bool showBanner,
                    out string successBannerContent);

                var viewModel = new FileListViewModel
                {
                    Title = DocumentsOptions.ListTitle,
                    UploadedFiles = uploadedFileViewModels,
                    CABId = id,
                    IsFromSummary = fromSummary,
                    ReturnUrl = returnUrl,
                    DocumentStatus = latestDocument.StatusValue,
                    SuccessBannerTitle = successBannerContent,
                    ShowBanner = showBanner
                };

                return View(viewModel);
            }

            uploadedFileViewModels = latestDocument.Documents?.Select(s => new FileViewModel
                {
                    FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, Category = s.Category,
                    Archived = s.Archived, Id = s.Id
                })
                .ToList() ?? new List<FileViewModel>();

            //Pre - populate model for edit
            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = uploadedFileViewModels,
                CABId = id,
                IsFromSummary = fromSummary,
                ReturnUrl = returnUrl,
                DocumentStatus = latestDocument.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, string submitType, FileUploadViewModel model,
            bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            latestDocument.Documents ??= new List<FileUpload>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.Cancel))
            {
                await AbortFileUploadAndReturnAsync(submitType, model, latestDocument,
                    nameof(latestDocument.Documents));

                if (fromSummary)
                {
                    return RedirectToAction("Summary", "CAB", new { id, revealEditActions = true });
                }
                else
                {
                    return RedirectToAction("CABManagement", "CABManagement",
                        new { Area = "admin", unlockCAb = latestDocument.CABId });
                }
            }

            if (submitType != null && submitType.Equals(Constants.SubmitType.Remove))
            {
                var FilesSelectedInViewModel = GetFilesSelectedInViewModel(model.UploadedFiles);

                if (!FilesSelectedInViewModel.Any())
                {
                    AddSelectAFileModelStateError(submitType, nameof(model.UploadedFiles), FilesSelectedInViewModel);
                }
                else
                {
                    var fileUploadsMatchingSelectedFiles =
                        _fileUploadUtils.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(FilesSelectedInViewModel,
                            latestDocument.Documents);
                    if (fileUploadsMatchingSelectedFiles.Any())
                    {
                        _fileUploadUtils.RemoveSelectedUploadedFilesFromDocumentAsync(fileUploadsMatchingSelectedFiles,
                            latestDocument, nameof(latestDocument.Documents));
                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                    }

                    var currentlyUploadedFileViewModels = latestDocument.Documents?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                        Category = s.Category, Archived = s.Archived, Id = s.Id
                    }).ToList() ?? new List<FileViewModel>();

                    var unsavedFileViewModels = model.UploadedFiles?.Where(u => u.IsDuplicated && !u.IsSelected)
                        .Select(s => new FileViewModel
                        {
                            FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                            Category = s.Category, IsSelected = false, Archived = s.Archived
                        }).ToList() ?? new List<FileViewModel>();
                    ;

                    currentlyUploadedFileViewModels.AddRange(unsavedFileViewModels);

                    return View(new FileListViewModel
                    {
                        Title = DocumentsOptions.ListTitle,
                        UploadedFiles = currentlyUploadedFileViewModels,
                        CABId = id,
                        IsFromSummary = fromSummary,
                        DocumentStatus = latestDocument.StatusValue
                    });
                }
            }

            AddCategoryLabelAndFileModelStateErrors(model);

            if (submitType != null && submitType.Equals(Constants.SubmitType.Continue))
            {
                AddCategorySelectionModelStateErrors(model);
            }

            if (ModelState.IsValid)
            {
                var userAccount =
                    await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                if (UpdateDocumentFiles(latestDocument, model.UploadedFiles))
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                }

                if (submitType == Constants.SubmitType.UploadAnother)
                {
                    return model.IsFromSummary
                        ? RedirectToAction("DocumentsUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId, fromSummary = true })
                        : RedirectToAction("DocumentsUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId });
                }

                if (submitType == Constants.SubmitType.Continue)
                {
                    return RedirectToAction("Summary", "CAB",
                        new { Area = "admin", id = latestDocument.CABId, revealEditActions = true });
                }

                if (submitType == Constants.SubmitType.Save)
                {
                    return SaveDraft(latestDocument);
                }

                if (submitType != null && submitType.Equals(Constants.SubmitType.UseFileAgain))
                {
                    return RedirectToAction("DocumentsUseFileAgain", "FileUploadManagement", new { id, fromSummary });
                }

                if (submitType != null && submitType.Equals(Constants.SubmitType.ReplaceFile))
                {
                    return RedirectToAction("DocumentsReplaceFile", "FileUploadManagement", new { id, fromSummary });
                }
            }

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = model.UploadedFiles ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue
            });
        }

        [HttpGet]
        [Route("admin/cab/schedules-list/remove/{id}/{scheduleId}/{actionType}", Name = Routes.SchedulesListRemove)]
        public async Task<IActionResult> SchedulesListRemove(string id, string scheduleId, RemoveActionEnum actionType, bool fromSummary)

        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var schedules = latestDocument?.Schedules ?? new();
            var fileUpload = schedules.Where(n => n.Id == Guid.Parse(scheduleId)).First() ?? throw new InvalidOperationException("No schedule found");
            var actionText = actionType == RemoveActionEnum.Remove ? "Remove" : "Archive";

            var vm = new RemoveScheduleViewModel
            {
                CabId = Guid.Parse(id),
                Title = $"{actionText} product schedule",
                RemoveScheduleAction = actionType,
                FileUpload = new Core.Domain.FileUpload(fileUpload.Id, fileUpload.Label, fileUpload.LegislativeArea, null, fileUpload.FileName, fileUpload.BlobName, fileUpload.UploadDateTime),
                IsFromSummary = fromSummary

            };

            return View("~/Areas/Admin/views/FileUpload/SchedulesRemove.cshtml", vm);
        
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/remove/{id}/{scheduleId}/{actionType}", Name = Routes.SchedulesListRemove)]
        public async Task<IActionResult> SchedulesListRemove(string id, string scheduleId, RemoveScheduleViewModel vm)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var schedules = latestDocument?.Schedules ?? new();
            var fileUpload = schedules.Where(n => n.Id == Guid.Parse(scheduleId)).First() ?? throw new InvalidOperationException("No schedule found"); 

            if (ModelState.IsValid)
            {
                var actionType = ProductScheduleActionMessageEnum.ProductScheduleRemoved;
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                var scheduleIds = new List<Guid> { Guid.Parse(scheduleId) };

                if (vm.RemoveScheduleAction == RemoveActionEnum.Remove)
                {
                    await _cabAdminService.RemoveSchedulesAsync(userAccount, Guid.Parse(id), scheduleIds);  
                }
                else
                {
                    await _cabAdminService.ArchiveSchedulesAsync(userAccount, Guid.Parse(id), scheduleIds);
                    actionType = ProductScheduleActionMessageEnum.ProductScheduleArchived;
                }

                return RedirectToAction("SchedulesList", new { id, actionType });
            }

            vm.FileUpload = new Core.Domain.FileUpload(fileUpload.Id, fileUpload.Label, fileUpload.LegislativeArea, null, fileUpload.FileName, fileUpload.BlobName, fileUpload.UploadDateTime);
            
            return View("~/Areas/Admin/views/FileUpload/SchedulesRemove.cshtml", vm);
        }
       

        [HttpGet]
        [Route("admin/cab/schedules-list/remove-option/{id}/{scheduleId}/{actionType}", Name = Routes.SchedulesListRemoveWithOption)]
        public async Task<IActionResult> SchedulesListRemoveOption(string id, string scheduleId, RemoveActionEnum actionType, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var schedules = latestDocument?.Schedules ?? new();
            var fileUpload = schedules.Where(n => n.Id == Guid.Parse(scheduleId)).First() ?? throw new InvalidOperationException("No schedule found");
            
            var actionText = actionType == RemoveActionEnum.Remove ? "Remove" : "Archive";

            // check if no legislative area assigned to schedule or legislative area have no other no product schedule linked to it
            var redirectToRemoveScheduleWithLARemoveAction = string.IsNullOrWhiteSpace(fileUpload.LegislativeArea) || schedules.Where(n => n.LegislativeArea == fileUpload.LegislativeArea).Count() <= 1;

            var vm = new RemoveScheduleWithOptionViewModel
            {
                CabId = Guid.Parse(id),
                Title = $"{actionText} product schedule",
                RemoveScheduleAction = actionType,
                FileUpload = new Core.Domain.FileUpload(fileUpload.Id, fileUpload.Label, fileUpload.LegislativeArea, null, fileUpload.FileName, fileUpload.BlobName, fileUpload.UploadDateTime),
                IsFromSummary = fromSummary
            };

            return View("~/Areas/Admin/views/FileUpload/SchedulesRemoveWithOption.cshtml", vm);
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/remove-option/{id}/{scheduleId}/{actionType}", Name = Routes.SchedulesListRemoveWithOption)]
        public async Task<IActionResult> SchedulesListRemoveOption(string id, string scheduleId, RemoveScheduleWithOptionViewModel vm)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var schedules = latestDocument?.Schedules ?? new();
            var fileUpload = schedules.Where(n => n.Id == Guid.Parse(scheduleId)).First() ?? throw new InvalidOperationException("No schedule found");
            var cabId = Guid.Parse(id);

            if (ModelState.IsValid)
            {
                var documentLegislativeArea = latestDocument?.DocumentLegislativeAreas.Where(n => n.LegislativeAreaName == fileUpload.LegislativeArea).First();
                var legislativeAreaId = documentLegislativeArea?.LegislativeAreaId;
                var actionType = ProductScheduleActionMessageEnum.ProductScheduleRemoved;
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);

                if (documentLegislativeArea != null && legislativeAreaId.HasValue)
                {
                    var laValue = legislativeAreaId.Value;

                    if (vm.RemoveLegislativeAction == LegislativeAreaActionEnum.Archive)
                    {
                        await _cabAdminService.ArchiveLegislativeAreaAsync(userAccount, cabId, laValue);
                    }
                    else if (vm.RemoveLegislativeAction == LegislativeAreaActionEnum.Remove)
                    {
                        await _cabAdminService.RemoveLegislativeAreaAsync(userAccount, cabId, laValue, fileUpload.LegislativeArea??string.Empty);
                    }
                    else
                    {
                        documentLegislativeArea.IsProvisional = true;                       
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
                    }
                }

                if (vm.RemoveScheduleAction == RemoveActionEnum.Remove)
                {   
                    await _cabAdminService.RemoveSchedulesAsync(userAccount, cabId, new List<Guid> { Guid.Parse(scheduleId) });

                    actionType = vm.RemoveLegislativeAction switch
                    {
                        LegislativeAreaActionEnum.Remove => ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaRemoved,
                        LegislativeAreaActionEnum.Archive => ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaArchived,     
                        _ => ProductScheduleActionMessageEnum.ProductScheduleRemovedLegislativeAreaProvisional,
                    };

                }
                else
                {
                    await _cabAdminService.ArchiveSchedulesAsync(userAccount, cabId, new List<Guid>() { Guid.Parse(scheduleId) });

                    actionType = vm.RemoveLegislativeAction switch
                    {
                        LegislativeAreaActionEnum.Remove => ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaRemoved,
                        LegislativeAreaActionEnum.Archive => ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaArchived,
                        _ => ProductScheduleActionMessageEnum.ProductScheduleArchivedLegislativeAreaProvisional,
                    };
                }

                return RedirectToAction("SchedulesList", new { id, actionType });
            }

            vm.FileUpload = new Core.Domain.FileUpload(fileUpload.Id, fileUpload.Label, fileUpload.LegislativeArea, null, fileUpload.FileName, fileUpload.BlobName, fileUpload.UploadDateTime);

            return View("~/Areas/Admin/views/FileUpload/SchedulesRemoveWithOption.cshtml", vm);
        }

        #region Private methods

        private bool ValidateUploadFile(IFormFile? file, string contentType, List<FileUpload> currentDocuments)
        {
            var isValidFile = _fileUploadUtils.ValidateUploadFileAndAddAnyModelStateError(ModelState, file, contentType,
                SchedulesOptions.AcceptedFileTypes);

            currentDocuments ??= new List<FileUpload>();

            if (currentDocuments.Any(s => s.FileName.Equals(file.FileName)))
            {
                ModelState.AddModelError("File",
                    $"{file.FileName} has already been uploaded. Select the existing file and the Use file again option, or upload a different file.");
                isValidFile = false;
            }

            return isValidFile;
        }

        private static void AddSelectedVMToUploadedFileViewModels(List<FileViewModel> uploadedFileViewModels,
            FileUpload selectedViewModel)
        {
            var uploadedFileToDuplicate = new FileViewModel
            {
                FileName = selectedViewModel.FileName,
                UploadDateTime = DateTime.UtcNow,
                Label = selectedViewModel.FileName,
                LegislativeArea = string.Empty,
                Category = string.Empty,
                IsSelected = false,
                IsDuplicated = true,
                Id = Guid.NewGuid()
            };

            uploadedFileViewModels.Add(uploadedFileToDuplicate);
        }

        private static void UpdateShowBannerAndSuccessBanner(List<FileViewModel>? uploadedFiles, out bool showBanner,
            out string successBannerContent)
        {
            showBanner = false;
            successBannerContent = "The file has been used again";
            if (uploadedFiles != null)
            {
                if (uploadedFiles.Any(f => f.IsDuplicated))
                {
                    showBanner = true;
                }
                else if (uploadedFiles.Any(f => f.IsReplaced))
                {
                    showBanner = true;
                    successBannerContent = "The replacement file has been uploaded";
                }
            }
        }

        private static void UpdateFileVMIsReplacedPropertyAndLoad(List<FileUpload> latestFileUploads,
            List<FileViewModel> uploadedFiles, int indexOfFileToReplace)
        {
            for (int i = 0; i < latestFileUploads.Count; i++)
            {
                var s = latestFileUploads[i];
                var uploadedfile = new FileViewModel
                {
                    FileName = s.FileName,
                    UploadDateTime = s.UploadDateTime,
                    Label = s.Label,
                    LegislativeArea = s.LegislativeArea,
                    Category = s.Category,
                    Id = s.Id,
                    IsReplaced = i == indexOfFileToReplace
                };
                uploadedFiles.Add(uploadedfile);
            }
        }

        private void AddLegislativeLabelAndFileModelStateErrors(ScheduleFileListViewModel model)
        {
            if (model.ActiveFiles != null && model.ActiveFiles.Any())
            {
                var duplicatedFileAndLabels = model.ActiveFiles
                    .Where(x => string.IsNullOrWhiteSpace(x.LegislativeArea) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { FileName = x.FileName.ToLower(), Label = x.Label!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (duplicatedFileAndLabels != null && duplicatedFileAndLabels.Count > 0)
                {
                    var index = 0;
                    foreach (var uploadedFile in model.ActiveFiles)
                    {
                        foreach (var uploadedFileLabel in duplicatedFileAndLabels)
                        {
                            var fileName = uploadedFileLabel.FileName;
                            var labelName = uploadedFileLabel.Label;

                            if (uploadedFile.Label!.Equals(labelName, StringComparison.OrdinalIgnoreCase) &&
                                uploadedFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"ActiveFiles[{index}].Label",
                                    "A file already exists with this title. Change the title or upload a different file.");
                            }
                        }

                        index++;
                    }
                }

                var duplicatedLabelsAndLegislativeAreas = model.ActiveFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { Label = x.Label!.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (duplicatedLabelsAndLegislativeAreas is { Count: > 0 })
                {
                    var index = 0;
                    foreach (var uploadedFile in model.ActiveFiles)
                    {
                        foreach (var uploadedFileLabel in duplicatedLabelsAndLegislativeAreas)
                        {
                            var labelName = uploadedFileLabel.Label;
                            var legislativeArea = uploadedFileLabel.LegislativeArea;

                            if (uploadedFile.Label!.Equals(labelName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.LegislativeArea ?? string.Empty).Equals(legislativeArea,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"ActiveFiles[{index}].Label",
                                    "A file associated with this legislative area is already using this title. Change the title of the file.");
                            }
                        }

                        index++;
                    }
                }

                var filesWithRepeatedNamesAndLegislativeAreas = model.ActiveFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea))
                    .GroupBy(x => new
                        { FileName = x.FileName.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (filesWithRepeatedNamesAndLegislativeAreas is { Count: > 0 })
                {
                    var index = 0;
                    foreach (var uploadedFile in model.ActiveFiles)
                    {
                        foreach (var repeatedFiles in filesWithRepeatedNamesAndLegislativeAreas)
                        {
                            var fileName = repeatedFiles.FileName;
                            var legislativeArea = repeatedFiles.LegislativeArea;

                            if (uploadedFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.LegislativeArea ?? string.Empty).Equals(legislativeArea,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"ActiveFiles[{index}].LegislativeArea",
                                    "The file is already associated with this legislative area.");
                            }
                        }

                        index++;
                    }
                }
            }
        }

        private void AddCategoryLabelAndFileModelStateErrors(FileUploadViewModel model)
        {
            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var duplicatedFileAndLabels = model.UploadedFiles
                    .Where(x => string.IsNullOrWhiteSpace(x.Category) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { FileName = x.FileName.ToLower(), Label = x.Label!.ToLower() })
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key).ToList();

                if (duplicatedFileAndLabels.Any())
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var uploadedFileLabel in duplicatedFileAndLabels)
                        {
                            var fileName = uploadedFileLabel.FileName;
                            var labelName = uploadedFileLabel.Label;

                            if (uploadedFile.Label!.Equals(labelName, StringComparison.OrdinalIgnoreCase) &&
                                uploadedFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label",
                                    "A file already exists with this title. Change the title.");
                            }
                        }

                        index++;
                    }
                }

                var duplicatedLabelsAndCategories = model.UploadedFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.Category) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { Label = x.Label!.ToLower(), Category = x.Category!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (duplicatedLabelsAndCategories.Any())
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var uploadedFileLabel in duplicatedLabelsAndCategories)
                        {
                            var labelName = uploadedFileLabel.Label;
                            var category = uploadedFileLabel.Category;

                            if (uploadedFile.Label!.Equals(labelName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.Category ?? string.Empty).Equals(category,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label",
                                    "A file associated with this category is already using this title. Change the title of the file.");
                            }
                        }

                        index++;
                    }
                }

                var filesWithRepeatedNamesAndCategories = model.UploadedFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.Category))
                    .GroupBy(x => new { FileName = x.FileName!.ToLower(), Category = x.Category!.ToLower() })
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key).ToList();

                if (filesWithRepeatedNamesAndCategories.Any())
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var repeatedFiles in filesWithRepeatedNamesAndCategories)
                        {
                            var fileName = repeatedFiles.FileName;
                            var category = repeatedFiles.Category;

                            if (uploadedFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.Category ?? string.Empty).Equals(category,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Category",
                                    "The file is already associated with this category.");
                            }
                        }

                        index++;
                    }
                }
            }
        }

        private void AddLegislativeSelectionModelStateErrors(ScheduleFileListViewModel model)
        {
            if (model.ActiveFiles != null && model.ActiveFiles.Any())
            {   
                if (model.ActiveFiles.Any(u => string.IsNullOrWhiteSpace(u.LegislativeArea)))
                {
                    var index = 0;
                    foreach (var uploadedFile in model.ActiveFiles)
                    {
                        if (string.IsNullOrWhiteSpace(uploadedFile.LegislativeArea))
                        {
                            ModelState.AddModelError($"ActiveFiles[{index}].LegislativeArea",
                                "Select a legislative area");
                        }

                        index++;
                    }
                }
            }
        }

        private void AddCategorySelectionModelStateErrors(FileUploadViewModel model)
        {
            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                if (model.UploadedFiles.Any(u => string.IsNullOrWhiteSpace(u.Category)))
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        if (string.IsNullOrWhiteSpace(uploadedFile.Category))
                        {
                            ModelState.AddModelError($"UploadedFiles[{index}].Category", "Select a category");
                        }

                        index++;
                    }
                }
            }
        }

        private static List<FileViewModel> GetFilesSelectedInViewModel(List<FileViewModel> filesInViewModel)
        {
            return filesInViewModel.Where(f => f.IsSelected).ToList() ?? new List<FileViewModel>();
        }

        private void AddSelectAFileModelStateError(string submitType, string modelKey,
            IEnumerable<FileViewModel> selectedViewModels)
        {
            if (submitType == Constants.SubmitType.Remove && !selectedViewModels.Any())
            {
                ModelState.AddModelError(modelKey, "Select a schedule to remove");
            }
        }

        private async Task AbortFileUploadAndReturnAsync(string submitType, FileUploadViewModel model,
            Document latestDocument, string docType)
        {
            if (submitType is Constants.SubmitType.Cancel)
            {
                var incompleteFileUploads = new List<FileViewModel>();

                if (latestDocument.Schedules != null && docType.Equals(nameof(latestDocument.Schedules)))
                {
                    if (model.UploadedFiles != null)
                    {
                        incompleteFileUploads = model.UploadedFiles
                            .Where(f => string.IsNullOrWhiteSpace(f.LegislativeArea)).ToList();
                    }

                    var selectedFileUploads =
                        _fileUploadUtils.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(incompleteFileUploads,
                            latestDocument.Schedules);
                    if (selectedFileUploads.Any())
                    {
                        _fileUploadUtils.RemoveSelectedUploadedFilesFromDocumentAsync(selectedFileUploads,
                            latestDocument, nameof(latestDocument.Schedules));
                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                    }
                }
                else if (latestDocument.Documents != null && docType.Equals(nameof(latestDocument.Documents)))
                {
                    if (model.UploadedFiles != null)
                    {
                        incompleteFileUploads = model.UploadedFiles.Where(f => string.IsNullOrWhiteSpace(f.Category))
                            .ToList();
                    }

                    var selectedFileUploads =
                        _fileUploadUtils.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(incompleteFileUploads,
                            latestDocument.Documents);
                    if (selectedFileUploads.Any())
                    {
                        _fileUploadUtils.RemoveSelectedUploadedFilesFromDocumentAsync(selectedFileUploads,
                            latestDocument, nameof(latestDocument.Documents));
                        var userAccount =
                            await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                .Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                    }
                }
            }
        }

        private static bool UpdateFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newSchedules = new List<FileUpload>();
            var activeSchedules = latestDocument?.Schedules?.Where(n => n.Archived is null or false);
            if (activeSchedules != null && activeSchedules.Any())
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var current = activeSchedules.First(fu => fu.Id.Equals(fileViewModel.Id));
                    newSchedules.Add(new FileUpload
                    {
                        Id = fileViewModel.Id,
                        FileName = fileViewModel.FileName,
                        BlobName = current.BlobName,
                        Label = fileViewModel.Label!,
                        LegislativeArea = fileViewModel.LegislativeArea,
                        UploadDateTime = current.UploadDateTime
                    });
                }
            }

            var fileUploadComparer = new FileUploadComparer();
            var updatedSchedules = newSchedules.Except(activeSchedules, fileUploadComparer);
            
            if (updatedSchedules.Any())
            {
                foreach(var update in updatedSchedules)
                {
                    var schedule = latestDocument.Schedules.First(fu => fu.Id.Equals(update.Id));

                    if (schedule != null)
                    {   
                        schedule.Label = update.Label;
                        schedule.LegislativeArea = update.LegislativeArea;
                    }
                }
                return true;
            }

            return false;
        }

        private static bool UpdateDocumentFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newDocuments = new List<FileUpload>();
            if (fileViewModels != null)
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var findById = latestDocument.Documents.FirstOrDefault(fu => fu.Id.Equals(fileViewModel.Id));
                    var findByFileName = latestDocument.Documents.FirstOrDefault(fu => fu.FileName.Equals(fileViewModel.FileName));

                    var uploadDateTime = fileViewModel.IsDuplicated ? DateTime.UtcNow : findByFileName?.UploadDateTime;
                    var blobName = findByFileName?.BlobName;

                    // check if record exists with the Id
                    if (findById != null)
                    {
                        uploadDateTime = findById.UploadDateTime;
                        blobName = findById.BlobName;
                    }                    

                    newDocuments.Add(new FileUpload
                    {
                        FileName = fileViewModel.FileName,
                        BlobName = blobName,
                        Label = fileViewModel.Label,
                        Category = fileViewModel.Category,
                        UploadDateTime = uploadDateTime??DateTime.UtcNow,
                        Id = fileViewModel.Id
                    });
                }
            }

            var fileUploadComparer = new FileUploadComparer();
            latestDocument.Documents ??= new();
            var newNotOld = newDocuments.Except(latestDocument.Documents, fileUploadComparer);
            var oldNotNew = latestDocument.Documents.Except(newDocuments, fileUploadComparer);
            if (newNotOld.Any() || oldNotNew.Any())
            {
                latestDocument.Documents = newDocuments;
                return true;
            }

            return false;
        }

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] =
                $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToAction("CABManagement", "CabManagement",
                new { Area = "admin", unlockCab = document.CABId });
        }

        private static IEnumerable<SelectListItem> GetDocumentAreaDistinctLegislativeAreas(Document latestDocument)
        {
            var legislativeareas = latestDocument.DocumentLegislativeAreas.Where(x => x.Archived is null or false)
               .Select(x => x.LegislativeAreaName)
               .Distinct();               
            
            List<SelectListItem> selectList = legislativeareas.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => new SelectListItem
            { Text = x, Value = x }).ToList();

            selectList.Insert(0,  new SelectListItem() { Text = Constants.NotAssigned, Value ="" });

            return selectList;
        }

        #endregion
    }
}