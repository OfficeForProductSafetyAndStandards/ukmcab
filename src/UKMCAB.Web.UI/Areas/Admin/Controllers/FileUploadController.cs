using Microsoft.AspNetCore.Authorization;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services;
using UKMCAB.Data.Models;
using UKMCAB.Data;
using UKMCAB.Data.Storage;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize(Policy = Policies.CabManagement)]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;

        public FileUploadController(ICABAdminService cabAdminService, IFileStorage fileStorage)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        [Route("admin/cab/schedules-upload/{id}")]
        public async Task<IActionResult> SchedulesUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            if (latestVersion.Schedules != null && latestVersion.Schedules.Count >= 5)
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            // Pre-populate model for edit
            latestVersion.Schedules ??= new List<FileUpload>();
            if (latestVersion.Schedules.Count >= 5)
            {
                return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id, fromSummary = "true" }: new { id });
            }

            var contentType = ValidateUploadFileAndGetContentType(model, SchedulesOptions.AcceptedFileExtensionsContentTypes, SchedulesOptions.AcceptedFileTypes, latestVersion.Schedules);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(latestVersion.CABId, model.File.FileName, model.File.FileName, DataConstants.Storage.Schedules,
                    model.File.OpenReadStream(), contentType);
                latestVersion.Schedules.Add(result);

                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
                return RedirectToAction("SchedulesList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            if (latestVersion.StatusValue == Status.Created)
            {
                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion, true);
            }
            TempData[Constants.TempDraftKey] = $"Draft record saved for {latestVersion.Name} <br>CAB number {latestVersion.CABNumber}";
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }

        private string ValidateUploadFileAndGetContentType(FileUploadViewModel model, Dictionary<string, string> acceptedFileExtensionsContentTypes, string acceptedFileTypes, List<FileUpload> currentDocuments)
        {
            var contentType = string.Empty;
            if (model.File == null)
            {
                ModelState.AddModelError("File", $"Select a {acceptedFileTypes} file 10 megabytes or less.");
            }
            else
            {
                if (model.File.Length > 10485760)
                {
                    ModelState.AddModelError("File", "Files must be no more that 10Mb in size.");
                }

                contentType = acceptedFileExtensionsContentTypes.SingleOrDefault(ct =>
                    ct.Key.Equals(Path.GetExtension(model.File.FileName), StringComparison.InvariantCultureIgnoreCase)).Value;
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    ModelState.AddModelError("File", $"Files must be in {acceptedFileTypes} format to be uploaded.");
                }

                currentDocuments ??= new List<FileUpload>();

                if (currentDocuments.Any(s => s.FileName.Equals(model.File.FileName)))
                {
                    ModelState.AddModelError("File", "Uploaded files must have different names to those already uploaded.");
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestDocument.Schedules ??= new List<FileUpload>();

            if (submitType.StartsWith("Remove") && Int32.TryParse(submitType.Replace("Remove-", String.Empty), out var fileIndex))
            {
                var fileToRemove = latestDocument.Schedules[fileIndex];

                latestDocument.Schedules.Remove(fileToRemove);
                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestDocument);
                return View(new FileListViewModel
                {
                    Title = SchedulesOptions.ListTitle,
                    UploadedFiles = latestDocument.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label }).ToList() ?? new List<FileViewModel>(),
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
                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                if (UpdateFiles(latestDocument, model.UploadedFiles))
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestDocument, submitType == Constants.SubmitType.Save);
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

        private IActionResult SaveDraft(Document document)
        {
            TempData[Constants.TempDraftKey] = $"Draft record saved for {document.Name} <br>CAB number {document.CABNumber}";
            return RedirectToAction("Index", "Admin", new { Area = "admin" });
        }


        [HttpGet]
        [Route("admin/cab/documents-upload/{id}")]
        public async Task<IActionResult> DocumentsUpload(string id, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
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
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label }).ToList() ?? new List<FileViewModel>(),
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();
            if (latestVersion.Documents.Count >= 10)
            {
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id, fromSummary = "true" } : new { id });
            }

            var contentType = ValidateUploadFileAndGetContentType(model, DocumentsOptions.AcceptedFileExtensionsContentTypes, DocumentsOptions.AcceptedFileTypes, latestVersion.Documents);

            if (ModelState.IsValid)
            {
                var result = await _fileStorage.UploadCABFile(latestVersion.CABId, model.File.FileName, model.File.FileName, DataConstants.Storage.Documents,
                    model.File.OpenReadStream(), contentType);
                latestVersion.Documents.Add(result);

                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
                return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
            }

            model.Title = DocumentsOptions.UploadTitle;
            model.UploadedFiles =
                latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label })
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
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/documents-list/{id}")]
        public async Task<IActionResult> DocumentsList(string id, string fileName, bool fromSummary)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("Index", "Admin", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();

            var fileToRemove = latestVersion.Documents.SingleOrDefault(s =>
                s.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
            if (fileToRemove != null)
            {
                //var result = await _fileStorage.DeleteCABSchedule(fileToRemove.BlobName);
                // Even if this returns false because the file wasn't found we still want to remove it from the document
                latestVersion.Documents.Remove(fileToRemove);
                var user = new Data.UKMCABUser();//TODO await _userManager.GetUserAsync(User);
                await _cabAdminService.UpdateOrCreateDraftDocumentAsync(user, latestVersion);
            }

            return View(new FileListViewModel
            {
                Title = DocumentsOptions.ListTitle,
                UploadedFiles = latestVersion.Documents?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label }).ToList() ?? new List<FileViewModel>(),
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

    }
}
