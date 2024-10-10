using System.Collections.Generic;
using NUnit.Framework;
using Moq;
using UKMCAB.Web.UI.Services;
using Microsoft.AspNetCore.Http;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB;
using UKMCAB.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using UKMCAB.Core.Services.CAB;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework.Legacy;

namespace UKMCAB.Web.UI.Tests
{
    [TestFixture]
    public class FileUploadUtilsTests
    {
        [Category("File Upload Utils Happy Path")]
        [Test]
        public void GetContentType_WithValid_File_ShouldReturn_CorrectContentType()
        {
            //Arrange
            var _sut = new FileUploadUtils();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("testfile.pdf");

            var acceptedFileExtensionsContentTypes = new Dictionary<string, string>
            {
                {".pdf", "application/pdf"}
            };

            //Act
            var result = _sut.GetContentType(fileMock.Object, acceptedFileExtensionsContentTypes);

            //Assert
            Assert.That(result, Is.EqualTo("application/pdf"));
        }

        [Category("File Upload Utils Happy Path")]
        [Test]
        public void GetContentType_WithNullFile_ShouldReturn_EmptyString()
        {
            //Arrange
            var _sut = new FileUploadUtils();

            var acceptedFileExtensionsContentTypes = new Dictionary<string, string>
            {
                {".pdf", "application/pdf"}
            };

            //Act
            var result = _sut.GetContentType(null, acceptedFileExtensionsContentTypes);

            //Assert
            Assert.That(result, Is.Empty);
        }

        [Category("File Upload Utils Sad Path")]
        [Test]
        public void GetContentType_WithInvalidFileExtension_ShouldReturn_Null()
        {
            //Arrange
            var _sut = new FileUploadUtils();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("testfile.doc");

            var acceptedFileExtensionsContentTypes = new Dictionary<string, string>
            {
                {".pdf", "application/pdf"}
            };

            //Act
            var result = _sut.GetContentType(fileMock.Object, acceptedFileExtensionsContentTypes);

            //Assert
            Assert.That(result, Is.Null);
        }

        [Category("File Upload Utils Happy Path")]
        [Test]
        public void GetSelectedFilesFromLatestDocumentOrReturnEmptyList_ShouldReturn_ListOfSelectedFileUploads()
        {
            //Arrange
            var _sut = new FileUploadUtils();
            var file1 = new FileUpload { FileName = "file1.pdf" };
            var file2 = new FileUpload { FileName = "file2.pdf" };
            var file3 = new FileUpload { FileName = "file3.pdf" };
            var file4 = new FileUpload { FileName = "file4.pdf" };

            IEnumerable<FileViewModel> filesSelectedByUser = new List<FileViewModel>()
            {
                new FileViewModel{ FileName = "file1.pdf", FileIndex = 0, IsDuplicated = false},
                new FileViewModel{ FileName = "file2.pdf", FileIndex = 1, IsDuplicated = false},
                new FileViewModel{ FileName = "file1.pdf", FileIndex = 2, IsDuplicated = true}
            };

            List<FileUpload> uploadedFilesInLatestDocument = new()
            {
                file1, file2, file3, file4
            };

            List<FileUpload> selectedUploadedFiles = new()
            {
                file1, file2
            };

            //Act
            var result = _sut.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(filesSelectedByUser, uploadedFilesInLatestDocument);


            //Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result, Is.EquivalentTo(selectedUploadedFiles));
        }

        [Category("File Upload Utils Sad Path")]
        [Test]
        public void GetSelectedFilesFromLatestDocumentOrReturnEmptyList_ShouldReturn_EmptyFileUploadList()
        {
            //Arrange
            var _sut = new FileUploadUtils();
            var file1 = new FileUpload { FileName = "file1.pdf" };
            var file2 = new FileUpload { FileName = "file2.pdf" };
            var file3 = new FileUpload { FileName = "file3.pdf" };
            var file4 = new FileUpload { FileName = "file4.pdf" };

            IEnumerable<FileViewModel> filesSelectedByUser = new List<FileViewModel>()
            {
                new FileViewModel{ FileName = "file1.pdf", FileIndex = 2, IsDuplicated = true}
            };

            List<FileUpload> uploadedFilesInLatestDocument = new()
            {
                file1, file2, file3, file4
            };

            List<FileUpload> selectedUploadedFiles = new();

            //Act
            var result = _sut.GetSelectedFilesFromLatestDocumentOrReturnEmptyList(filesSelectedByUser, uploadedFilesInLatestDocument);


            //Assert
            Assert.That(result.Count, Is.EqualTo(0));
            Assert.That(result, Is.EquivalentTo(selectedUploadedFiles));
        }


        [Category("File Upload Utils Happy Path")]
        [Test]
        public void RemoveSelectedUploadedFilesFromDocumentAsync_ShouldRemoveSelectedFileUploads()
        {
            //Arrange
            var _sut = new FileUploadUtils();
            var file1 = new FileUpload { FileName = "file1.pdf" };
            var file2 = new FileUpload { FileName = "file2.pdf" };
            var file3 = new FileUpload { FileName = "file3.pdf" };
            var file4 = new FileUpload { FileName = "file4.pdf" };


            List<FileUpload> uploadedFilesInLatestDocument = new()
            {
                file1, file2, file3, file4
            };

            List<FileUpload> selectedUploadedFiles = new()
            {
                file1, file2
            };

            List<FileUpload> returnedFiles = new()
            {
                file3, file4
            };

            var latestdoc = new Document() { CABNumber = "test-cab", Schedules = uploadedFilesInLatestDocument };

            //Act
            var result = _sut.RemoveSelectedUploadedFilesFromDocumentAsync(selectedUploadedFiles, latestdoc, "Schedules");


            //Assert
            Assert.That(uploadedFilesInLatestDocument.Count, Is.EqualTo(2));
            Assert.That(latestdoc.Schedules.First().FileName, Is.EquivalentTo("file3.pdf"));
        }

        [Category("File Upload Utils Sad Path")]
        [Test]
        public async Task RemoveSelectedUploadedFilesFromDocumentAsync_ShouldNotRemoveAnyFile_WhenThereIsNoMatch()
        {
            //Arrange
            var cabServiceMock = new Mock<ICABAdminService>();
            var _sut = new FileUploadUtils();
            var file1 = new FileUpload { FileName = "file1.pdf" };
            var file2 = new FileUpload { FileName = "file2.pdf" };
            var file3 = new FileUpload { FileName = "file3.pdf" };
            var file4 = new FileUpload { FileName = "file4.pdf" };

            List<FileUpload> uploadedFilesInLatestDocument = new()
            {
                file1, file2, file3
            };

            List<FileUpload> selectedUploadedFiles = new()
            {
                file4
            };

            var latestdoc = new Document() { CABNumber = "test-cab", Schedules = uploadedFilesInLatestDocument };            

            //Act            
            _sut.RemoveSelectedUploadedFilesFromDocumentAsync(selectedUploadedFiles, latestdoc, "Schedules");

            //Assert
            Assert.That(uploadedFilesInLatestDocument.Count, Is.EqualTo(3));
            Assert.That(latestdoc.Schedules.First().FileName, Is.EquivalentTo("file1.pdf"));
        }

        [Category("File Upload Utils Sad Path")]
        [Test]
        public void ValidateUploadFileAndAddAnyModelStateError_WithNullFile_AddsModelErrorAndReturnsFalse()
        {
            // Arrange
            var _sut = new FileUploadUtils();

            var modelState = new ModelStateDictionary();
            IFormFile file = null;
            var contentType = "application/pdf";
            var acceptedFileTypes = "PDF";

            // Act
            var result = _sut.ValidateUploadFileAndAddAnyModelStateError(modelState, file, contentType, acceptedFileTypes);

            // Assert
            Assert.That(result, Is.False);
            CollectionAssert.Contains(modelState.Keys.ToList(), "File");
            Assert.That($"Select a {acceptedFileTypes} file 10 megabytes or less.", Is.EqualTo(modelState["File"].Errors[0].ErrorMessage));
        }

        [Category("File Upload Utils Sad Path")]
        [Test]
        public void ValidateUploadFileAndAddAnyModelStateError_WithInvalidFile_AddsModelErrorAndReturnsFalse()
        {
            // Arrange
            var _sut = new FileUploadUtils();

            var modelState = new ModelStateDictionary();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(20000000); // File length greater than 10485760
            fileMock.Setup(f => f.FileName).Returns("appointmentletter.doc");
            var contentType = "";
            var acceptedFileTypes = "Doc";

            // Act
            var result = _sut.ValidateUploadFileAndAddAnyModelStateError(modelState, fileMock.Object, contentType, acceptedFileTypes);

            // Assert
            Assert.That(result, Is.False);
            CollectionAssert.Contains(modelState.Keys.ToList(), "File");
            Assert.That("appointmentletter.doc can't be uploaded. Select a Doc file 10 megabytes or less.", Is.EqualTo(modelState["File"].Errors[0].ErrorMessage));
            Assert.That("appointmentletter.doc can't be uploaded. Files must be in Doc format to be uploaded.", Is.EqualTo(modelState["File"].Errors[1].ErrorMessage));
        }

        [Category("File Upload Utils Happy Path")]
        [Test]
        public void ValidateUploadFileAndAddAnyModelStateError_WithValidFile_ReturnsTrueAndDoesNotAddModelError()
        {
            // Arrange
            var _sut = new FileUploadUtils();

            var modelState = new ModelStateDictionary();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(5000000); // File length less than 10485760
            fileMock.Setup(f => f.FileName).Returns("schedule.pdf");
            var contentType = "application/pdf";
            var acceptedFileTypes = "PDF";

            // Act
            var result = _sut.ValidateUploadFileAndAddAnyModelStateError(modelState, fileMock.Object, contentType, acceptedFileTypes);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(modelState.ContainsKey("File"), Is.False);
        }
    }
}
