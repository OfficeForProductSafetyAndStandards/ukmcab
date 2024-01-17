using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Web;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadManagementController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;

        public static class Routes
        {
            //public const string SchedulesList = "file-upload.schedules-list";
        }

        public FileUploadManagementController(ICABAdminService cabAdminService, IFileStorage fileStorage, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userService = userService;
        }

        

        private string GetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes)
        {
            if (file == null)
            {
                return string.Empty;
            }

            return acceptedFileExtensionsContentTypes.SingleOrDefault(ct =>
                    ct.Key.Equals(Path.GetExtension(file.FileName), StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        private bool ValidateUploadFile(IFormFile? file, string contentType, string acceptedFileTypes, List<FileUpload> currentDocuments)
        {
            var isValidFile = true;

            if (file == null)
            {
                ModelState.AddModelError("File", $"Select a {acceptedFileTypes} file 10 megabytes or less.");
                return false;
            }
            else
            {
                if (file.Length > 10485760)
                {
                    ModelState.AddModelError("File", $"{file.FileName} can't be uploaded. Select a {acceptedFileTypes} file 10 megabytes or less.");
                    isValidFile = false;
                }

                if (string.IsNullOrWhiteSpace(contentType))
                {
                    ModelState.AddModelError("File", $"{file.FileName} can't be uploaded. Files must be in {acceptedFileTypes} format to be uploaded.");
                    isValidFile = false;
                }

                currentDocuments ??= new List<FileUpload>();

                if (currentDocuments.Any(s => s.FileName.Equals(file.FileName)))
                {
                    ModelState.AddModelError("File", $"{file.FileName} has already been uploaded. Select the existing file and the Use file again option, or upload a different file.");
                    isValidFile = false;
                }
            }
            return isValidFile;
        }

        

        private void AddLabelAndLegislativeModelStateErrors(FileUploadViewModel model)
        {
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

            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var duplicatedLabels = model.UploadedFiles.GroupBy(x => x.Label).Where(g => g.Count() > 1)
                                        .Select(y => y.Key).ToList();

                if (duplicatedLabels.Count > 0)
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var uploadedLabel in duplicatedLabels)
                        {
                            if (!string.IsNullOrWhiteSpace(uploadedFile.Label) && uploadedFile.Label.Equals(uploadedLabel) && uploadedFile.IsDuplicated)
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label", "A file already exists with this title. Change the title or upload a different file.");
                            }
                        }
                        index++;
                    }
                }
            }
        }

        [HttpGet("admin/cab/schedules-use-file-again/{id}")]
        public async Task<IActionResult> SchedulesUseFileAgain(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new FileUploadViewModel
            {
                Title = SchedulesOptions.UseFileAgainTitle,
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost("admin/cab/schedules-use-file-again/{id}")]
        public async Task<IActionResult> SchedulesUseFileAgain(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            latestDocument.Schedules ??= new List<FileUpload>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.UseFileAgain) && model.IndexofSelectedFile == null)
            {
                ModelState.AddModelError(nameof(model.IndexofSelectedFile), "Select the file you want to use again");
                return View(new FileUploadViewModel
                {
                    Title = SchedulesOptions.UseFileAgainTitle,
                    UploadedFiles = latestDocument.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestDocument.StatusValue
                });
            }

            return RedirectToAction("SchedulesList", "FileUpload", new { id, fromSummary, model.IndexofSelectedFile, fromAction = nameof(SchedulesUseFileAgain) });
        }
        private List<FileViewModel> GetFilesSelectedInViewModel(List<FileViewModel> filesInViewModel)
        {

            return filesInViewModel.Where(f => f.IsSelected).ToList() ?? new List<FileViewModel>();
        }
        private List<FileUpload> GetSelectedFilesFromLatestDocumentOrReturnEmptyList(IEnumerable<FileViewModel> selectedViewModels, List<FileUpload> uploadedFiles)
        {
            List<FileUpload> selectedFileUploads = new();

            if (selectedViewModels.Any())
            {
                foreach (var fileVM in selectedViewModels)
                {
                    if (!fileVM.IsDuplicated)
                    {
                        selectedFileUploads.Add(uploadedFiles[fileVM.FileIndex]);
                    }
                }
            }

            return selectedFileUploads;
        }
        private void AddSelectAFileModelStateError(string submitType, string modelKey, IEnumerable<FileViewModel> selectedViewModels)
        {
            if (submitType == Constants.SubmitType.Remove && !selectedViewModels.Any())
            {
                ModelState.AddModelError(modelKey, "Select a file to remove");
            }
        }
        private async Task RemoveSelectedUploadedFilesFromDocumentAsync(List<FileUpload> selectedFileUploads, Document latestDocument, string docType)
        {
            if (latestDocument.Schedules != null && docType.Equals(nameof(latestDocument.Schedules)))
            {
                foreach (var fileToRemove in selectedFileUploads)
                {
                    latestDocument.Schedules.Remove(fileToRemove);
                }
            }
            else if (latestDocument.Documents != null && docType.Equals(nameof(latestDocument.Documents)))
            {
                foreach (var fileToRemove in selectedFileUploads)
                {
                    latestDocument.Documents.Remove(fileToRemove);
                }
            }

            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
        }

        private async Task AbortFileUploadAndReturnAsync(string submitType, FileUploadViewModel model, Document latestDocument, string docType)
        {
            if (submitType != null && submitType.Equals(Constants.SubmitType.Cancel))
            {
                var incompleteFileUploads = new List<FileViewModel>();
                
                if (latestDocument.Schedules != null && docType.Equals(nameof(latestDocument.Schedules)))
                {
                    if (model.UploadedFiles != null)
                    {
                        incompleteFileUploads = model.UploadedFiles.Where(f => string.IsNullOrWhiteSpace(f.LegislativeArea)).ToList();
                    }

                    var selectedFileUploads = GetSelectedFilesFromLatestDocumentOrReturnEmptyList(incompleteFileUploads, latestDocument.Schedules);
                    if (selectedFileUploads.Any())
                    {
                        await RemoveSelectedUploadedFilesFromDocumentAsync(selectedFileUploads, latestDocument, nameof(latestDocument.Schedules));
                    }
                }
                else if (latestDocument.Documents != null && docType.Equals(nameof(latestDocument.Documents)))
                {
                    if (model.UploadedFiles != null)
                    {
                        incompleteFileUploads = model.UploadedFiles.Where(f => string.IsNullOrWhiteSpace(f.Category)).ToList();
                    }
                    var selectedFileUploads = GetSelectedFilesFromLatestDocumentOrReturnEmptyList(incompleteFileUploads, latestDocument.Documents);
                    if (selectedFileUploads.Any())
                    {
                        await RemoveSelectedUploadedFilesFromDocumentAsync(selectedFileUploads, latestDocument, nameof(latestDocument.Documents));
                    }
                }
            }
        }
        private bool UpdateFiles(Document latestDocument, List<FileViewModel> fileViewModels)
        {
            var newSchedules = new List<FileUpload>();
            if (fileViewModels != null && latestDocument.Schedules != null)
            {
                foreach (var fileViewModel in fileViewModels)
                {
                    var current = latestDocument.Schedules.First(fu => fu.FileName.Equals(fileViewModel.FileName));
                    newSchedules.Add(new FileUpload
                    {
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
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin", unlockCab = document.CABId });
        }

        [HttpGet("admin/cab/replace-file/{id}")]
        public async Task<IActionResult> ReplaceFile(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new FileUploadViewModel
            {
                Title = SchedulesOptions.ReplaceFile,
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost("admin/cab/replace-file/{id}")]
        public async Task<IActionResult> ReplaceFile(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            var schedules = latestDocument.Schedules;
            schedules ??= new List<FileUpload>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.ReplaceFile) && model.IndexofSelectedFile == null)
            {
                if (model.CABId != null && model.IndexofSelectedFile == null)
                {
                    ModelState.AddModelError(nameof(model.IndexofSelectedFile), "Select the file you want to replace");
                }
            }

            if (submitType != null && submitType.Equals(Constants.SubmitType.ReplaceFile))
            {
                var file = model.File;
                var contentType = GetContentType(file, SchedulesOptions.AcceptedFileExtensionsContentTypes);

                if (ValidateUploadFile(file, contentType, SchedulesOptions.AcceptedFileTypes, schedules))
                {
                    await UploadAndReplaceWithValidatedFile(model, latestDocument, schedules, file, contentType, DataConstants.Storage.Schedules);
                }
            }

            if (ModelState.IsValid)
            {
                if (submitType != null && submitType.Equals(Constants.SubmitType.Continue))
                {
                    return model.IsFromSummary ?
                            RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestDocument.CABId, subSectionEditAllowed = true }) :
                            RedirectToAction("DocumentsUpload", "FileUpload", new { Area = "admin", id = latestDocument.CABId });
                }

                return RedirectToAction("SchedulesList", "FileUploadController", new { id, fromSummary, model.IndexofSelectedFile, fromAction = nameof(ReplaceFile) });
            }

            model.Title = SchedulesOptions.ReplaceFile;
            return View(model);
        }

        private async Task UploadAndReplaceWithValidatedFile(FileUploadViewModel model, Document? latestDocument, List<FileUpload>? latestUploadedFiles, IFormFile? file, string contentType, string directoryName)
        {
            var replacementUploadedSchedule = await _fileStorage.UploadCABFile(latestDocument.CABId, file.FileName, file.FileName, directoryName,
                file.OpenReadStream(), contentType);
            if (latestDocument.Schedules != null && int.TryParse(model.IndexofSelectedFile, out var indexOfFileToReplace) && indexOfFileToReplace < latestUploadedFiles.Count)
            {
                var scheduleToReplace = latestUploadedFiles[indexOfFileToReplace];
                
                var sb = new StringBuilder();
                var oldFileLink = $"/search/cab-schedule-view/{latestDocument.CABId}?file={scheduleToReplace.FileName}&filetype=schedules";
                var newFileLink = $"/search/cab-schedule-view/{latestDocument.CABId}?file={replacementUploadedSchedule.FileName}&filetype=schedules";

                sb.AppendFormat("<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">Old file</a></p>", oldFileLink);
                sb.AppendFormat("<p class=\"govuk-body\"><a href=\"{0}\" target=\"_blank\" class=\"govuk-link\">New file</a></p>", newFileLink);

                scheduleToReplace.FileName = replacementUploadedSchedule.FileName;
                scheduleToReplace.BlobName = replacementUploadedSchedule.BlobName;
                scheduleToReplace.UploadDateTime = replacementUploadedSchedule.UploadDateTime;

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                var auditLog = new Audit(userAccount, AuditCABActions.Created, null, HttpUtility.HtmlEncode(sb.ToString()), false);
                latestDocument.AuditLog.Add(auditLog);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
            }
        }

        

        private void AddLabelAndCategoryModelStateErrors(FileUploadViewModel model)
        {
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

            if (model.UploadedFiles != null && model.UploadedFiles.Any())
            {
                var duplicatedLabels = model.UploadedFiles.GroupBy(x => x.Label).Where(g => g.Count() > 1)
                                        .Select(y => y.Key).ToList();

                if (duplicatedLabels.Count > 0)
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var uploadedLabel in duplicatedLabels)
                        {
                            if (uploadedFile.Label!.Equals(uploadedLabel) && uploadedFile.IsDuplicated)
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label", "A file already exists with this title. Change the title or upload a different file.");
                            }
                        }
                        index++;
                    }
                }

                var filesWithRepeatedNamesAndCategories = model.UploadedFiles.GroupBy(x => new { x.FileName, x.Category }).Where(g => g.Count() > 1)
                                        .Select(y => y.Key).ToList();

                if (filesWithRepeatedNamesAndCategories != null && filesWithRepeatedNamesAndCategories.Count > 0)
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach (var repeatedFiles in filesWithRepeatedNamesAndCategories)
                        {
                            var fileName = repeatedFiles.FileName;
                            var category = repeatedFiles.Category;

                            if (uploadedFile.FileName.Equals(fileName) && uploadedFile.Category!.Equals(category) && uploadedFile.IsDuplicated)
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Category", "The file already exists in this category.");
                            }
                        }
                        index++;
                    }
                }
            }
        }

        [HttpGet("admin/cab/documents-use-file-again/{id}")]
        public async Task<IActionResult> DocumentsUseFileAgain(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new FileUploadViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, Category = s.Category?.Trim() }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost("admin/cab/documents-use-file-again/{id}")]
        public async Task<IActionResult> DocumentsUseFileAgain(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            latestDocument.Documents ??= new List<FileUpload>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.UseFileAgain) && model.IndexofSelectedFile == null)
            {
                ModelState.AddModelError(nameof(model.IndexofSelectedFile), "Select the file you want to use again");
                return View(new FileUploadViewModel
                {
                    Title = DocumentsOptions.ListTitle,
                    UploadedFiles = latestDocument.Documents?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, Category = s.Category }).ToList() ?? new List<FileViewModel>(),
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestDocument.StatusValue
                });
            }

            return RedirectToAction("DocumentsList", "FileUpload", new { id, fromSummary, fileIndexToDuplicate = model.IndexofSelectedFile });
        }
    }
}
