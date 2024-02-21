using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Infrastructure.Cache;
using UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Enums;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.Schedule;
using UKMCAB.Web.UI.Services;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;
        private readonly IFileUploadUtils _fileUploadUtils;
        private readonly ILegislativeAreaService _legislativeAreaService;
        private readonly IDistCache _distCache;

        public static class Routes
        {
            public const string SchedulesList = "file-upload.schedules-list";
            public const string SchedulesListRemove = "file-upload.schedules-list-remove";
        }

        public FileUploadController(
            ICABAdminService cabAdminService,
            IFileStorage fileStorage,
            IUserService userService,
            IFileUploadUtils fileUploadUtils,
            ILegislativeAreaService legislativeAreaService,
            IDistCache distCache)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userService = userService;
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
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel
                {
                    FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                    LegislativeArea = s.LegislativeArea
                }).ToList() ?? new List<FileViewModel>(),
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

                    if (ValidateUploadFile(file, contentType, SchedulesOptions.AcceptedFileTypes,
                            latestVersion.Schedules))
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
            model.UploadedFiles =
                latestVersion.Schedules?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                        LegislativeArea = s.LegislativeArea, Id = s.Id, Archived = s.Archived
                })
                    .ToList() ?? new List<FileViewModel>();
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
        public async Task<IActionResult> SchedulesList(string id, bool fromSummary, string? indexOfSelectedFile,
            string? fromAction)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            if (!latestVersion.DocumentLegislativeAreas.Any(a => a.Archived is null or false))
            {
                return RedirectToRoute(LegislativeAreaDetailsController.Routes.AddLegislativeArea, new { id });
            }

            var uploadedFileViewModels = new List<FileViewModel>();

            if (fromAction is nameof(FileUploadManagementController.SchedulesReplaceFile)
                or nameof(FileUploadManagementController.SchedulesUseFileAgain))
            {
                if (fromAction == nameof(FileUploadManagementController.SchedulesReplaceFile) &&
                    latestVersion.Schedules != null &&
                    int.TryParse(indexOfSelectedFile, out var indexOfFileToReplace) &&
                    indexOfFileToReplace < latestVersion.Schedules.Count)
                {
                    UpdateFileVMIsReplacedPropertyAndLoad(latestVersion.Schedules, uploadedFileViewModels,
                        indexOfFileToReplace);
                }

                if (fromAction == nameof(FileUploadManagementController.SchedulesUseFileAgain) &&
                    latestVersion.Schedules != null && int.TryParse(indexOfSelectedFile, out var fileToUseAgainIndex) &&
                    fileToUseAgainIndex < latestVersion.Schedules.Count)
                {
                    uploadedFileViewModels = latestVersion.Schedules?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                        LegislativeArea = s.LegislativeArea?.Trim(), Archived = s.Archived, Id = s.Id
                    }).ToList() ?? new List<FileViewModel>();

                    var selectedViewModel = latestVersion.Schedules[fileToUseAgainIndex];
                    AddSelectedVMToUploadedFileViewModels(uploadedFileViewModels, selectedViewModel);
                }

                UpdateShowBannerAndSuccessBanner(uploadedFileViewModels, out bool showBanner,
                    out string successBannerContent);
                var viewModel = new FileListViewModel
                {
                    Title = SchedulesOptions.ListTitle,
                    UploadedFiles = uploadedFileViewModels,
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestVersion.StatusValue,
                    SuccessBannerTitle = successBannerContent,
                    ShowBanner = showBanner,
                    LegislativeAreas = latestVersion.LegislativeAreas.ToList()
                };

                return View(viewModel);
            }

            uploadedFileViewModels = latestVersion.Schedules?.Select(s => new FileViewModel
            {
                FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                LegislativeArea = s.LegislativeArea?.Trim(), Archived = s.Archived, Id = s.Id
            }).ToList() ?? new List<FileViewModel>();

            var legislativeArea = latestVersion.DocumentLegislativeAreas.Where(a => a.Archived is null or false)
                .Select(a => a.LegislativeAreaName)
                .Distinct()
                .ToList();

            // Pre-populate model for edit
            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = uploadedFileViewModels,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue,
                LegislativeAreas = legislativeArea
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}", Name = Routes.SchedulesList)]
        public async Task<IActionResult> SchedulesList(string id, string submitType, FileUploadViewModel model,
            bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            latestDocument.Schedules ??= new List<FileUpload>();

            if (submitType is Constants.SubmitType.Cancel)
            {
                await AbortFileUploadAndReturnAsync(submitType, model, latestDocument,
                    nameof(latestDocument.Schedules));

                if (fromSummary)
                {
                    return RedirectToAction("Summary", "CAB", new { id, subSectionEditAllowed = true });
                }
                else
                {
                    return RedirectToAction("CABManagement", "CABManagement", new { Area = "admin", unlockCab = id });
                }
            }

            if (submitType is Constants.SubmitType.Remove)
            {
                var filesInViewModel = model.UploadedFiles ?? new List<FileViewModel>();

                var selectedViewModels = GetFilesSelectedInViewModel(filesInViewModel);
                if (!selectedViewModels.Any())
                {
                    AddSelectAFileModelStateError(submitType, nameof(model.UploadedFiles), selectedViewModels);
                }
                else
                {
                    var cabDocuments = await _cabAdminService.FindDocumentsByCABIdAsync(id.ToString());

                    var fileUploadsInLatestDocumentMatchingUserSelection =
                        _fileUploadUtils.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(selectedViewModels,
                            latestDocument.Schedules);

                    if (fileUploadsInLatestDocumentMatchingUserSelection.Any())
                    {
                        // only one document and draft mode then remove the schedule else give user an option to remove or archive the schedule
                        if (cabDocuments.Count == 1 && cabDocuments.First().StatusValue == Status.Draft)
                        {
                            _fileUploadUtils.RemoveSelectedUploadedFilesFromDocumentAsync(
                                fileUploadsInLatestDocumentMatchingUserSelection, latestDocument,
                                nameof(latestDocument.Schedules));
                            var userAccount =
                                await _userService.GetAsync(User.Claims
                                    .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                                    .Value);
                            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
                        }
                        // ask user to remove or archive
                        else
                        {
                            var storagekey = Guid.NewGuid().ToString();
                            await _distCache.SetAsync(storagekey,
                                fileUploadsInLatestDocumentMatchingUserSelection.Select(n => n.Id).ToList(),
                                TimeSpan.FromHours(1));

                            return RedirectToRoute(Routes.SchedulesListRemove, new { id, storagekey });
                        }
                    }

                    var currentlyUploadedFileViewModels = latestDocument.Schedules?.Select(s => new FileViewModel
                    {
                        FileName = s.FileName,
                        UploadDateTime = s.UploadDateTime,
                        Label = s.Label,
                        LegislativeArea = s.LegislativeArea,
                        IsSelected = false,
                        Archived = s.Archived,
                        Id = s.Id
                    }).ToList() ?? new List<FileViewModel>();

                    var unsavedFileViewModels = model.UploadedFiles?.Where(u => u.IsDuplicated && !u.IsSelected)
                        .Select(s => new FileViewModel
                        {
                            FileName = s.FileName,
                            UploadDateTime = s.UploadDateTime,
                            Label = s.Label,
                            LegislativeArea = s.LegislativeArea,
                            IsSelected = false,
                            Archived = s.Archived
                        }).ToList() ?? new List<FileViewModel>();
                    ;

                    currentlyUploadedFileViewModels.AddRange(unsavedFileViewModels);

                    return View(new FileListViewModel
                    {
                        Title = SchedulesOptions.ListTitle,
                        UploadedFiles = currentlyUploadedFileViewModels,
                        CABId = id,
                        IsFromSummary = fromSummary,
                        DocumentStatus = latestDocument.StatusValue
                    });
                }
            }

            AddLegislativeLabelAndFileModelStateErrors(model);

            if (submitType is Constants.SubmitType.Continue)
            {
                AddLegislativeSelectionModelStateErrors(model);
            }

            if (ModelState.IsValid)
            {
                if (UpdateFiles(latestDocument, model.UploadedFiles))
                {
                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value);
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount!, latestDocument);
                }

                if (submitType == Constants.SubmitType.UploadAnother)
                {
                    return model.IsFromSummary
                        ? RedirectToAction("SchedulesUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId, fromSummary = true })
                        : RedirectToAction("SchedulesUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId });
                }

                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary
                        ? RedirectToAction("Summary", "CAB",
                            new { Area = "admin", id = latestDocument.CABId, subSectionEditAllowed = true })
                        : RedirectToAction("DocumentsUpload", "FileUpload",
                            new { Area = "admin", id = latestDocument.CABId });
                }

                if (submitType == Constants.SubmitType.Save)
                {
                    return SaveDraft(latestDocument);
                }

                if (submitType is Constants.SubmitType.UseFileAgain)
                {
                    return RedirectToAction("SchedulesUseFileAgain", "FileUploadManagement", new { id, fromSummary });
                }

                if (submitType is Constants.SubmitType.ReplaceFile)
                {
                    return RedirectToAction("SchedulesReplaceFile", "FileUploadManagement", new { id, fromSummary });
                }
            }

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = model.UploadedFiles,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue,
                LegislativeAreas = latestDocument.LegislativeAreas.ToList()
            });
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

                    if (ValidateUploadFile(file, contentType, DocumentsOptions.AcceptedFileTypes,
                            latestVersion.Documents))
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
        public async Task<IActionResult> DocumentsList(string id, bool fromSummary, string? indexOfSelectedFile,
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
                    return RedirectToAction("Summary", "CAB", new { id, subSectionEditAllowed = true });
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
                        new { Area = "admin", id = latestDocument.CABId, subSectionEditAllowed = true });
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
        [Route("admin/cab/schedules-list/remove/{id}/{storageKey}", Name = Routes.SchedulesListRemove)]
        public async Task<IActionResult> SchedulesListRemove(string id, string storageKey)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var fileUploadListIds = await _distCache.GetAsync<List<Guid>>(storageKey);

            var vm = new RemoveScheduleViewModel
            {
                CabId = Guid.Parse(id),  
                Title = "CAB Remove Schedules",
                ScheduleFileLabelList = latestVersion.Schedules.Where(n => fileUploadListIds.Contains(n.Id))
                    .Select(n => n.Label).ToList()
            };

            return View("~/Areas/Admin/views/FileUpload/SchedulesRemove.cshtml", vm);
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/remove/{id}/{storageKey}", Name = Routes.SchedulesListRemove)]
        public async Task<IActionResult> SchedulesListRemove(string id, string storageKey, RemoveScheduleViewModel vm)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var fileUploadListIds = await _distCache.GetAsync<List<Guid>>(storageKey);

            if (ModelState.IsValid)
            {
                List<FileUpload> selectedSchedules =
                    _fileUploadUtils.GetSelectedFilesFromLatestDocumentByIds(fileUploadListIds,
                        latestDocument.Schedules);

                if (vm.Action == RemoveActionEnum.Remove)
                {
                    _fileUploadUtils.RemoveSelectedUploadedFilesFromDocumentAsync(selectedSchedules, latestDocument,
                        nameof(latestDocument.Schedules));

                    var userAccount =
                        await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
                            .Value);
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                }
                else
                {
                    await _cabAdminService.ArchiveSchedulesAsync(Guid.Parse(id), fileUploadListIds);
                }

                _distCache.Remove(storageKey);

                return RedirectToAction("SchedulesList", new { id });
            }

            vm.ScheduleFileLabelList = latestDocument.Schedules.Where(n => fileUploadListIds.Contains(n.Id))
                .Select(n => n.Label).ToList();
            return View("~/Areas/Admin/views/FileUpload/SchedulesRemove.cshtml", vm);
        }

        #region Private methods

        private bool ValidateUploadFile(IFormFile? file, string contentType, string acceptedFileTypes,
            List<FileUpload> currentDocuments)
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
                UploadDateTime = selectedViewModel.UploadDateTime,
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
                    FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label,
                    LegislativeArea = s.LegislativeArea, Category = s.Category, Id = s.Id,
                };
                uploadedfile.IsReplaced = i == indexOfFileToReplace;
                uploadedFiles.Add(uploadedfile);
            }
        }

        private void AddLegislativeLabelAndFileModelStateErrors(FileUploadViewModel model)
        {
            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var duplicatedFileAndLabels = model.UploadedFiles
                    .Where(x => string.IsNullOrWhiteSpace(x.LegislativeArea) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { FileName = x.FileName.ToLower(), Label = x.Label!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (duplicatedFileAndLabels != null && duplicatedFileAndLabels.Count > 0)
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
                                    "A file already exists with this title. Change the title or upload a different file.");
                            }
                        }

                        index++;
                    }
                }

                var duplicatedLabelsAndLegislativeAreas = model.UploadedFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea) && !string.IsNullOrWhiteSpace(x.Label))
                    .GroupBy(x => new { Label = x.Label!.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (duplicatedLabelsAndLegislativeAreas is { Count: > 0 })
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var uploadedFileLabel in duplicatedLabelsAndLegislativeAreas)
                        {
                            var labelName = uploadedFileLabel.Label;
                            var legislativeArea = uploadedFileLabel.LegislativeArea;

                            if (uploadedFile.Label!.Equals(labelName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.LegislativeArea ?? string.Empty).Equals(legislativeArea,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label",
                                    "A label already exists in this legislative area. Change the title or legislative area.");
                            }
                        }

                        index++;
                    }
                }

                var filesWithRepeatedNamesAndLegislativeAreas = model.UploadedFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea))
                    .GroupBy(x => new
                        { FileName = x.FileName.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower() })
                    .Where(g => g.Count() > 1).Select(y => y.Key).ToList();

                if (filesWithRepeatedNamesAndLegislativeAreas is { Count: > 0 })
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var repeatedFiles in filesWithRepeatedNamesAndLegislativeAreas)
                        {
                            var fileName = repeatedFiles.FileName;
                            var legislativeArea = repeatedFiles.LegislativeArea;

                            if (uploadedFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                (uploadedFile.LegislativeArea ?? string.Empty).Equals(legislativeArea,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].LegislativeArea",
                                    "The file already exists in this legislative area.");
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
                                    "A file already exists with this title. Change the title or upload a different file.");
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
                                    "A label already exists in this category. Change the title or category.");
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
                                    "The file already exists in this category.");
                            }
                        }

                        index++;
                    }
                }
            }
        }

        private void AddLegislativeSelectionModelStateErrors(FileUploadViewModel model)
        {
            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var unArchivedUploadedFiles = model.UploadedFiles.Where(n => !n.Archived.GetValueOrDefault());

                if (unArchivedUploadedFiles.Any(u => string.IsNullOrWhiteSpace(u.LegislativeArea)))
                {
                    var index = 0;
                    foreach (var uploadedFile in unArchivedUploadedFiles)
                    {
                        if (string.IsNullOrWhiteSpace(uploadedFile.LegislativeArea))
                        {
                            ModelState.AddModelError($"UploadedFiles[{index}].LegislativeArea",
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

        private List<FileViewModel> GetFilesSelectedInViewModel(List<FileViewModel> filesInViewModel)
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

        private bool UpdateFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newSchedules = new List<FileUpload>();
            if (latestDocument.Schedules != null && latestDocument.Schedules.Any())
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var current = latestDocument.Schedules.First(fu => fu.FileName.Equals(fileViewModel.FileName));
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

            if (newSchedules.Any())
            {
                var legislativeAreasFromDocs = newSchedules.Select(sch => sch.LegislativeArea).ToList();

                if (legislativeAreasFromDocs.Except(latestDocument.LegislativeAreas).Any())
                {
                    var newLaList = legislativeAreasFromDocs.Union(latestDocument.LegislativeAreas).OrderBy(la => la)
                        .ToList();
                    latestDocument.LegislativeAreas = newLaList;
                }
            }

            var fileUploadComparer = new FileUploadComparer();
            var newNotOld = newSchedules.Except(latestDocument.Schedules, fileUploadComparer);
            var oldNotNew = latestDocument.Schedules.Except(newSchedules, fileUploadComparer);
            if (newNotOld.Any() || oldNotNew.Any())
            {
                latestDocument.Schedules = newSchedules;
                return true;
            }

            return false;
        }

        private bool UpdateDocumentFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newDocuments = new List<FileUpload>();
            if (fileViewModels != null)
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var current = latestDocument.Documents.First(fu => fu.FileName.Equals(fileViewModel.FileName));
                    newDocuments.Add(new FileUpload
                    {
                        FileName = fileViewModel.FileName,
                        BlobName = current.BlobName,
                        Label = fileViewModel.Label,
                        Category = fileViewModel.Category,
                        UploadDateTime = current.UploadDateTime,
                        Id = fileViewModel.Id
                    });
                }
            }

            var fileUploadComparer = new FileUploadComparer();
            latestDocument.Schedules ??= new();
            var newNotOld = newDocuments.Except(latestDocument.Schedules, fileUploadComparer);
            var oldNotNew = latestDocument.Schedules.Except(newDocuments, fileUploadComparer);
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

        #endregion
    }
}