using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Security.Claims;
using UKMCAB.Core.Security;
using UKMCAB.Core.Services.CAB;
using UKMCAB.Core.Services.Users;
using UKMCAB.Data;
using UKMCAB.Data.Models;
using UKMCAB.Data.Storage;
using UKMCAB.Web.UI.Models.ViewModels.Admin;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using static UKMCAB.Web.UI.Constants;
using Document = UKMCAB.Data.Models.Document;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Authorize]
    public class FileUploadController : Controller
    {
        private readonly ICABAdminService _cabAdminService;
        private readonly IFileStorage _fileStorage;
        private readonly IUserService _userService;

        public static class Routes
        {
            public const string SchedulesList = "file-upload.schedules-list";
        }

            public FileUploadController(ICABAdminService cabAdminService, IFileStorage fileStorage, IUserService userService)
        {
            _cabAdminService = cabAdminService;
            _fileStorage = fileStorage;
            _userService = userService;
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
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
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

                foreach (var file in model.Files)
                {
                    
                    var contentType = GetContentType(file, SchedulesOptions.AcceptedFileExtensionsContentTypes);

                    if(ValidateUploadFile(file, contentType, SchedulesOptions.AcceptedFileTypes, latestVersion.Schedules))
                    {
                        var result = await _fileStorage.UploadCABFile(latestVersion.CABId, file.FileName, file.FileName, DataConstants.Storage.Schedules,
                            file.OpenReadStream(), contentType);
                        latestVersion.Schedules.Add(result);

                        var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                    }
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
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion, false);
            TempData[Constants.TempDraftKey] = $"Draft record saved for {latestVersion.Name} <br>CAB number {latestVersion.CABNumber}";
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
        }

        private string GetContentType(IFormFile? file, Dictionary<string, string> acceptedFileExtensionsContentTypes)
        {
            if(file == null)
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

        [HttpGet]
        [Route("admin/cab/schedules-list/{id}", Name = Routes.SchedulesList)]
        public async Task<IActionResult> SchedulesList(string id, bool fromSummary, string? fileIndexToDuplicate)
        {
            var latestVersion = await _cabAdminService.GetLatestDocumentAsync(id);
            if (latestVersion == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }

            var uploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>();

            if (uploadedFiles != null && int.TryParse(fileIndexToDuplicate, out var fileToUseAgainIndex) && fileToUseAgainIndex < uploadedFiles.Count) 
            {
                var selectedViewModel = latestVersion.Schedules[fileToUseAgainIndex];
                var uploadedFileToDuplicate = new FileViewModel
                {
                    FileName = selectedViewModel.FileName,
                    Label = selectedViewModel.FileName,
                    LegislativeArea = string.Empty,
                    IsSelected = false,
                    IsDuplicated = true,
                };

                uploadedFiles.Add(uploadedFileToDuplicate);
            }




            // Pre-populate model for edit
            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = uploadedFiles,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestVersion.StatusValue
            });
        }

        [HttpPost]
        [Route("admin/cab/schedules-list/{id}", Name = Routes.SchedulesList)]
        public async Task<IActionResult> SchedulesList(string id, string submitType, FileUploadViewModel model, bool fromSummary)
        {
            var latestDocument = await _cabAdminService.GetLatestDocumentAsync(id);
            
            if (latestDocument == null) // Implies no document or archived
            {
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            latestDocument.Schedules ??= new List<FileUpload>();

            var filesInViewModel = model.UploadedFiles ?? new List<FileViewModel>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.UseFileAgain) && model.FileToUseAgain == null)
            {
                ModelState.AddModelError(nameof(model.FileToUseAgain), "Select the file you want to use again");
                return RedirectToAction("SchedulesUseFileAgain", "FileUpload", new { id, fromSummary });
            }


                if (submitType != null && submitType.Equals(Constants.SubmitType.Remove))
            {
                var selectedViewModels = GetSelectedFileViewModels(filesInViewModel);
                //if (!selectedFileUploads.Any())
                if (!selectedViewModels.Any())
                {
                    AddSelectAFileModelStateError(submitType, nameof(model.UploadedFiles), selectedViewModels);
                }
                else
                {
                    var selectedFileUploads = GetSelectedFilesFromLatestDocumentOrEmptyList(selectedViewModels, latestDocument);
                    if (selectedFileUploads.Any())
                    {
                        RemoveSelectedUploadedFilesFromDocument(submitType, selectedFileUploads, latestDocument);
                    }                    

                    var currentlyUploadedFileViewModels = latestDocument.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea, IsSelected = false }).ToList() ?? new List<FileViewModel>();

                    var unsavedFileViewModels = model.UploadedFiles?.Where(u => u.IsDuplicated && !u.IsSelected).Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea, IsSelected = false }).ToList() ?? new List<FileViewModel>(); ;

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

            if(filesInViewModel.Any())
            {
                var duplicatedLabels = model.UploadedFiles.GroupBy(x => x.Label).Where(g => g.Count() > 1)
                                        .Select(y => y.Key).ToList();

                if(duplicatedLabels.Count > 0)
                {
                    var index = 0;
                    foreach (var uploadedFile in model.UploadedFiles)
                    {
                        foreach(var uploadedLabel in duplicatedLabels)
                        {
                            if (uploadedFile.Label.Equals(uploadedLabel) && uploadedFile.IsDuplicated)
                            {
                                ModelState.AddModelError($"UploadedFiles[{index}].Label", "A file already exists with this title. Change the title or upload a different file.");
                            }
                        }

                        index++;
                    }
                }

            }

            var duplicatedFiles = new List<FileViewModel>();
            var updatedUploadedFiles = new List<FileViewModel>();

            if (ModelState.IsValid)
            {
                var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                if (UpdateFiles(latestDocument, model.UploadedFiles))
                {
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, User.IsInRole(Roles.UKAS.Id) && submitType == Constants.SubmitType.Save);
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
               
                //if (submitType == Constants.SubmitType.UseFileAgain)
                //{
                //    var selectedViewModel = new FileViewModel();

                //    if (int.TryParse(model.FileToUseAgain, out var fileToUseAgainIndex))
                //    {
                //        selectedViewModel = model.UploadedFiles[fileToUseAgainIndex];
                //    }
                    
                //    var uploadedFileToDuplicate = new FileViewModel
                //    {
                //        FileName = selectedViewModel.FileName,
                //        Label = selectedViewModel.FileName,
                //        LegislativeArea = string.Empty,
                //        IsSelected = false,
                //        IsDuplicated = true,
                //    };

                //    duplicatedFiles.Add(uploadedFileToDuplicate);

                //}

                if (submitType == Constants.SubmitType.Save)
                {
                    return SaveDraft(latestDocument);
                }
            }

            if (filesInViewModel.Any())
            {
                foreach (var file in filesInViewModel)
                {
                    updatedUploadedFiles.Add(file);
                }
            }
            if (duplicatedFiles.Any())
            {
                updatedUploadedFiles.AddRange(duplicatedFiles);
            }
            
            return View(new FileListViewModel
            {
                Title = SchedulesOptions.ListTitle,
                UploadedFiles = updatedUploadedFiles,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue,
                IsValidState = ModelState.IsValid
            });
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
                UploadedFiles = latestVersion.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
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

            var filesInViewModel = model.UploadedFiles ?? new List<FileViewModel>();

            if (submitType != null && submitType.Equals(Constants.SubmitType.UseFileAgain) && model.FileToUseAgain == null)
            {
                ModelState.AddModelError(nameof(model.FileToUseAgain), "Select the file you want to use again");
                return View(new FileUploadViewModel
                {
                    Title = SchedulesOptions.UseFileAgainTitle,
                    UploadedFiles = latestDocument.Schedules?.Select(s => new FileViewModel { FileName = s.FileName, Label = s.Label, LegislativeArea = s.LegislativeArea?.Trim() }).ToList() ?? new List<FileViewModel>(),
                    CABId = id,
                    IsFromSummary = fromSummary,
                    DocumentStatus = latestDocument.StatusValue
                });
            }

            return RedirectToAction("SchedulesList", "FileUpload", new { id, fromSummary, fileIndexToDuplicate = model.FileToUseAgain});
        }
        private List<FileViewModel> GetSelectedFileViewModels(List<FileViewModel> filesInViewModel)
        {

            return filesInViewModel.Where(f => f.IsSelected).ToList() ?? new List<FileViewModel>();
        }
        private List<FileUpload> GetSelectedFilesFromLatestDocumentOrEmptyList(IEnumerable<FileViewModel> selectedViewModels, Document latestDocument)
        {
            List<FileUpload> selectedFileUploads = new List<FileUpload>();

            if (selectedViewModels.Any())
            {

                foreach (var fileVM in selectedViewModels)
                {
                    if (!fileVM.IsDuplicated)
                    {
                        selectedFileUploads.Add(latestDocument.Schedules[fileVM.FileIndex]);
                    }

                }
            }

            return selectedFileUploads;
        }


        private void AddSelectAFileModelStateError(string submitType, string modelKey, IEnumerable<FileViewModel> selectedViewModels)
        {
            if ((submitType == Constants.SubmitType.Remove) && !selectedViewModels.Any())
            {
                ModelState.AddModelError(modelKey, "Select a file to remove");
            }
        }

        private async Task RemoveSelectedUploadedFilesFromDocument(string submitType, List<FileUpload> selectedFileUploads, Document latestDocument)
        {
            var currentlyUploadedFiles = new List<FileViewModel>();


            foreach (var fileToRemove in selectedFileUploads)
            {
                latestDocument.Schedules.Remove(fileToRemove);
            }

            var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
            await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument);


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
            return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
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
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
            }
            latestVersion.Documents ??= new List<FileUpload>();

            if (model.Files != null)
            {
                foreach (var file in model.Files)
                {
                    var contentType = GetContentType(file, DocumentsOptions.AcceptedFileExtensionsContentTypes);

                    if(ValidateUploadFile(file, contentType, DocumentsOptions.AcceptedFileTypes, latestVersion.Documents))
                    {
                        var result = await _fileStorage.UploadCABFile(latestVersion.CABId, file.FileName, file.FileName, DataConstants.Storage.Schedules,
                            file.OpenReadStream(), contentType);
                        latestVersion.Documents.Add(result);
                        if (latestVersion.Documents.Count > 1)
                        {
                            latestVersion.Documents.Sort((x, y) => DateTime.Compare(y.UploadDateTime, x.UploadDateTime));
                        }

                        var userAccount = await _userService.GetAsync(User.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value);
                        await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestVersion);
                    }
                }

                if (ModelState.IsValid)
                {
                    return RedirectToAction("DocumentsList", model.IsFromSummary ? new { id = latestVersion.CABId, fromSummary = "true" } : new { id = latestVersion.CABId });
                }
            }
            else if (model.Files == null && model.File == null)
            {
                ModelState.AddModelError("File", $"Select a {DocumentsOptions.AcceptedFileTypes} file 10 megabytes or less.");
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
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
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
                return RedirectToAction("CABManagement", "CabManagement", new { Area = "admin" });
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
                    await _cabAdminService.UpdateOrCreateDraftDocumentAsync(userAccount, latestDocument, User.IsInRole(Roles.UKAS.Id) && submitType == Constants.SubmitType.Save);
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
                UploadedFiles = model.UploadedFiles,
                CABId = id,
                IsFromSummary = fromSummary,
                DocumentStatus = latestDocument.StatusValue
            });
        }

    }
}
