using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Web.UI.Services;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadManagementController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;
        private readonly IFileUploadUtils _fileUploadUtils;

        public FileUploadManagementController(ICABAdminService cabAdminService, IFileStorage fileStorage, IUserService userService, IFileUploadUtils fileUploadUtils)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userService = userService;
            _fileUploadUtils = fileUploadUtils;
        }       

        [HttpGet("admin/cab/schedules-replace-file/{id}/{scheduleId}")]
        public async Task<IActionResult> SchedulesReplaceFile(string id, string scheduleId)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new FileUploadViewModel
            {
                Title = SchedulesOptions.ReplaceFile,
                UploadedFiles = latestVersion.Schedules?.Where(n => n.Id == Guid.Parse(scheduleId)).Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost("admin/cab/schedules-replace-file/{id}/{scheduleId}")]
        public async Task<IActionResult> SchedulesReplaceFile(string id, string scheduleId, FileUploadViewModel model)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var file = model.File;
            var contentType = _fileUploadUtils.GetContentType(file, SchedulesOptions.AcceptedFileExtensionsContentTypes);

            if (_fileUploadUtils.ValidateUploadFileAndAddAnyModelStateError(ModelState, file, contentType, SchedulesOptions.AcceptedFileTypes))
            {   
                await UploadAndReplaceScheduleWithValidatedFile(scheduleId, latestDocument, file, contentType, DataConstants.Storage.Schedules);
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("SchedulesList", "FileUpload", new { id, IndexofSelectedFile = model.IndexofSelectedFile, fromAction = "SchedulesReplaceFile" });
            }

            model.Title = SchedulesOptions.ReplaceFile;
            return View(model);
        }

        [HttpGet("admin/cab/documents-replace-file/{id}")]
        public async Task<IActionResult> DocumentsReplaceFile(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            return View(new FileUploadViewModel
            {
                Title = DocumentsOptions.ReplaceFile,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, UploadDateTime = s.UploadDateTime, Label = s.Label, Category = s.Category}).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost("admin/cab/documents-replace-file/{id}")]
        public async Task<IActionResult> DocumentsReplaceFile(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);

            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

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
                var contentType = _fileUploadUtils.GetContentType(file, DocumentsOptions.AcceptedFileExtensionsContentTypes);

                if (_fileUploadUtils.ValidateUploadFileAndAddAnyModelStateError(ModelState, file, contentType, DocumentsOptions.AcceptedFileTypes))
                {
                    await UploadAndReplaceWithValidatedFile(model, latestVersion, file, contentType, DataConstants.Storage.Documents);
                }
            }

            if (ModelState.IsValid)
            {
                if (submitType != null && submitType.Equals(Constants.SubmitType.Continue))
                {
                    return model.IsFromSummary ?
                            RedirectToAction("Summary", "CAB", new { Area = "admin", id = latestVersion.CABId, subSectionEditAllowed = true }) :
                            RedirectToAction("DocumentsUpload", "FileUpload", new { Area = "admin", id = latestVersion.CABId });
                }

                return RedirectToAction("DocumentsList", "FileUpload", new { id, fromSummary, model.IndexofSelectedFile, fromAction = "DocumentsReplaceFile" });
            }

            model.Title = DocumentsOptions.ReplaceFile;
            return View(model);
        }

        private async Task UploadAndReplaceWithValidatedFile(FileUploadViewModel model, Document? latestDocument, IFormFile? file, string contentType, string directoryName)
        {
            var newFileName = AppendDateTimeToFileName(DateTime.UtcNow, file.FileName);

            var replacementUploadedSchedule = await _fileStorage.UploadCABFile(latestDocument.CABId, file.FileName, newFileName, directoryName,
                file.OpenReadStream(), contentType);

            var latestFileUploads = new List<FileUpload>();
            latestFileUploads = directoryName == DataConstants.Storage.Schedules ? latestDocument.Schedules : latestDocument.Documents;

            if (latestFileUploads != null && int.TryParse(model.IndexofSelectedFile, out var indexOfFileToReplace) && indexOfFileToReplace < latestFileUploads.Count)
            {
                var scheduleToReplace = latestFileUploads[indexOfFileToReplace];
                
                scheduleToReplace.FileName = replacementUploadedSchedule.FileName;
                scheduleToReplace.BlobName = replacementUploadedSchedule.BlobName;
                scheduleToReplace.UploadDateTime = replacementUploadedSchedule.UploadDateTime;

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
            }
        }

        private async Task UploadAndReplaceScheduleWithValidatedFile(string scheduleId, Document? latestDocument, IFormFile? file, string contentType, string directoryName)
        {
            var newFileName = AppendDateTimeToFileName(DateTime.UtcNow, file.FileName);

            var replacementUploadedSchedule = await _fileStorage.UploadCABFile(latestDocument.CABId, file.FileName, newFileName, directoryName,
                file.OpenReadStream(), contentType);

            var scheduleToReplace = latestDocument.Schedules?.Where(n => n.Id == Guid.Parse(scheduleId)).FirstOrDefault();
            
            if (scheduleToReplace != null)
            {
                scheduleToReplace.FileName = replacementUploadedSchedule.FileName;
                scheduleToReplace.BlobName = replacementUploadedSchedule.BlobName;
                scheduleToReplace.UploadDateTime = replacementUploadedSchedule.UploadDateTime;

                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);
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

            return RedirectToAction("DocumentsList", "FileUpload", new { id, fromSummary, model.IndexofSelectedFile, fromAction = nameof(DocumentsUseFileAgain) });
        }

        private static string AppendDateTimeToFileName(DateTime date, string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName);
            string formattedDateTime = date.ToString("yyyyMMddHHmmss");

            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);

            string newFileName = $"{fileNameWithoutExtension}-{formattedDateTime}{extension}";

            return newFileName;
        }
    }
}
