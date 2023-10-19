using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;

        public FileUploadController(ICABAdminService cabAdminService, IFileStorage fileStorage, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userService = userService;
        }

        [HttpGet]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            if (latestVersion.Schedules != null && latestVersion.Schedules.Count >= SchedulesOptions.MaxFileCount)
            {
                return RedirectToAction("SchedulesList", fromSummary ? new {id, fromSummary = "true"} : new { id });
            }

            var model = new FileUploadViewModel
            {
                Title = SchedulesOptions.UploadTitle,
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel{FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea}).ToList() ?? new List<FileViewModel>(),
                CABId = id
            };
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpPost]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            latestVersion.Schedules ??= new List<FileUpload>();
            if (latestVersion.Schedules.Count >= SchedulesOptions.MaxFileCount)
            {
                return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id, fromSummary = "true" }: new { id });
            }

            var allowableNoOfFiles = SchedulesOptions.MaxFileCount - latestVersion.Schedules.Count;

            if (model.Files != null && model.Files.Count <= allowableNoOfFiles)
            {
                var errorCount = 0;

                foreach (var file in model.Files)
                {
                    var contentType = ValidateUploadFileAndGetContentType(file, SchedulesOptions.AcceptedFileExtensionsContentTypes, SchedulesOptions.AcceptedFileTypes, latestVersion.Schedules);

                    if (ModelState.ErrorCount == errorCount)
                    {
                        var result = await _fileStorage.UploadCABFile(latestVersion.CABId, file.FileName, file.FileName, DataConstants.Storage.Schedules,
                            file.OpenReadStream(), contentType);
                        latestVersion.Schedules.Add(result);

                        var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                    }

                    errorCount = ModelState.ErrorCount;
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
                }
                
            }
            else if(model.Files != null && model.Files.Count > allowableNoOfFiles)
            {
                ModelState.AddModelError("File", $"Max upload is 35. You can only upload {allowableNoOfFiles} file(s) more.");
            }
            else if (model.Files == null && model.File == null)
            {
                ModelState.AddModelError("File", $"Select a {SchedulesOptions.AcceptedFileTypes} file 10 megabytes or less.");
            }
            model.Title = SchedulesOptions.UploadTitle;
            model.UploadedFiles =
                latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea})
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
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion, true);
            TempData[Constants.TempDraftKey] = $"Draft record saved for {latestVersion.Name} <br>CAB number {latestVersion.CABNumber}";
            return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
        }

        private string ValidateUploadFileAndGetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes, string acceptedFileTypes, List<FileUpload> currentDocuments)
        {
            var contentType = string.Empty;
            if (file == null)
            {
                ModelState.AddModelError("File", $"Select a {acceptedFileTypes} file 10 megabytes or less.");
            }
            else
            {
                if (file.Length > 10485760)
                {
                    ModelState.AddModelError("File", $"{file.FileName} can't be uploaded. Select a {acceptedFileTypes} file 10 megabytes or less.");
                }

                contentType = acceptedFileExtensionsContentTypes.SingleOrDefault(ct =>
                    ct.Key.Equals(Path.GetExtension(file.FileName), StringComparison.InvariantCultureIgnoreCase)).Value;

                if (string.IsNullOrWhiteSpace(contentType))
                {
                   ModelState.AddModelError("File", $"{file.FileName} can't be uploaded. Files must be in {acceptedFileTypes} format to be uploaded.");
                }

                currentDocuments ??= new List<FileUpload>();

                if (currentDocuments.Any(s => s.FileName.Equals(file.FileName)))
                {
                   ModelState.AddModelError("File", $"{file.FileName} can't be uploaded. Uploaded files must have different names to those already uploaded.");
                }             
            }

            return contentType;
        }

        [HttpGet]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim()}).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}")]
        public async Task<IActionResult> SchedulesList(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            latestDocument.Schedules ??= new List<FileUpload>();

            if (submitType != null && submitType.StartsWith("Remove") && Int32.TryParse(submitType.Replace("Remove-", String.Empty), out var fileIndex))
            {
                var fileToRemove = latestDocument.Schedules[fileIndex];

                latestDocument.Schedules.Remove(fileToRemove);
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                return View(new FileListViewModel
                {
                    Title = SchedulesOptions.ListTitle,
                    UploadedFiles = latestDocument.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea }).ToList() ?? new List<FileViewModel>(),
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestDocument.StatusValue
                });
            }

            if (model.UploadedFiles != null && model.UploadedFiles.Any(u => string.IsNullOrWhiteSpace(u.LegislativeArea)))
            {
                var index = 0;
                foreach (var uploadedFile in model.UploadedFiles)
                {
                    if (string.IsNullOrWhiteSpace(uploadedFile.LegislativeArea))
                    {
                        ModelState.AddModelError($"UploadedFiles[{index}].LegislativeArea", "Select a legislative area");
                    }

                    index++;
                }
            }

            if (ModelState.IsValid)
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                if (UpdateFiles(latestDocument, model.UploadedFiles))
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                }
                if (submitType == Constants.SubmitType.UploadAnother)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("SchedulesUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId, fromSummary = true }) :
                        RedirectToAction("SchedulesUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }
                
                if (submitType == Constants.SubmitType.Continue)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId }) :
                        RedirectToAction("DocumentsUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = model.UploadedFiles,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue
            });
        }

        private bool UpdateFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newSchedules = new List<FileUpload>();
            if (fileViewModels != null)
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var current = latestDocument.Schedules.First(fu => fu.FileName.Equals(fileViewModel.FileName));
                    newSchedules.Add(new FileUpload
                    {
                        FileName = fileViewModel.FileName,
                        BlobName = current.BlobName,
                        Label = fileViewModel.Label, 
                        LegislativeArea = fileViewModel.LegislativeArea,
                        UploadDateTime = current.UploadDateTime
                    });
                }
            }

            if (newSchedules.Any())
            {
                if (latestDocument.LegislativeAreas == null)
                {
                    latestDocument.LegislativeAreas = new List<string>();
                }
                var legislativeAreasFromDocs = newSchedules.Select(sch => sch.LegislativeArea).ToList();
                
                if (legislativeAreasFromDocs.Except(latestDocument.LegislativeAreas).Any())
                {
                    var newLAList = legislativeAreasFromDocs.Union(latestDocument.LegislativeAreas).OrderBy(la => la).ToList();
                    latestDocument.LegislativeAreas = newLAList;
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
                        UploadDateTime = current.UploadDateTime
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
            TempData[Constants.TempDraftKey] = $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
        }


        [HttpGet]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            latestVersion.Documents ??= new List<FileUpload>();
            if (latestVersion.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", fromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var model = new FileUploadViewModel()
            {
                Title = DocumentsOptions.UploadTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, Category = s.Category}).ToList() ?? new List<FileViewModel>(),
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
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();
            if (latestVersion.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var contentType = ValidateUploadFileAndGetContentType(model.File, DocumentsOptions.AcceptedFileExtensionsContentTypes, DocumentsOptions.AcceptedFileTypes, latestVersion.Documents);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(latestVersion.CABId, model.File.FileName, model.File.FileName, DataConstants.Storage.Documents,
                    model.File.OpenReadStream(), contentType);
                latestVersion.Documents.Add(result);
                if(latestVersion.Documents.Count > 1)
                {
                    latestVersion.Documents.Sort((x, y) => DateTime.Compare(y.UploadDateTime, x.UploadDateTime));
                }


                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
            }

            model.Title = DocumentsOptions.UploadTitle;
            model.UploadedFiles =
                latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, Category = s.Category})
                    .ToList() ?? new List<FileViewModel>();
            model.CABId = id;
            model.IsFromSummary = fromSummary;
            model.DocumentStatus = latestVersion.StatusValue;
            return View(model);
        }

        [HttpGet]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, Category = s.Category?.Trim()}).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "Admin", new { Area = "admin" });
            }
            latestDocument.Documents ??= new List<FileUpload>();

            if (submitType != null && submitType.StartsWith("Remove") && Int32.TryParse(submitType.Replace("Remove-", String.Empty), out var fileIndex))
            {
                var fileToRemove = latestDocument.Documents[fileIndex];

                latestDocument.Documents.Remove(fileToRemove);
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
                return View(new FileListViewModel
                {
                    Title = DocumentsOptions.ListTitle,
                    UploadedFiles = latestDocument.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, Category = s.Category }).ToList() ?? new List<FileViewModel>(),
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestDocument.StatusValue
                });
            }



            if (model.UploadedFiles != null && model.UploadedFiles.Any(u => string.IsNullOrWhiteSpace(u.Category)))
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



            if (ModelState.IsValid)
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                if (UpdateDocumentFiles(latestDocument, model.UploadedFiles))
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, submitType == Constants.SubmitType.Save);
                }
                if (submitType == Constants.SubmitType.UploadAnother)
                {
                    return model.IsFromSummary ?
                        RedirectToAction("DocumentsUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId, fromSummary = true }) :
                        RedirectToAction("DocumentsUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }

                if (submitType == Constants.SubmitType.Continue)
                {
                    return RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId });
                }
                return SaveDraft(latestDocument);
            }

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestDocument.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, Category = s.Category}).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue
            });
        }

    }
}
