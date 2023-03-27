using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using UKMCAB.Common.Exceptions;
using UKMCAB.Core.Models;
using UKMCAB.Core.Services;
using UKMCAB.Infrastructure.Logging;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Data.Storage
{
    public class FileStorageService : IFileStorage
    {
        private readonly ILoggingService _loggingService;
        private BlobContainerClient _client;
        public FileStorageService(IConfiguration config, ILoggingService loggingService)
        {
            _loggingService = loggingService;
            _client = new BlobContainerClient(config["DataConnectionString"], config["CABFileStorageContainer"]);
            _client.CreateIfNotExists();
        }

        public async Task<FileDownload> DownloadBlobStream(string blobPath)
        {
            var blob = _client.GetBlobClient(blobPath);
            var blobExists = await blob.ExistsAsync();
            if (blobExists.HasValue && blobExists.Value)
            {
                var properties = await blob.GetPropertiesAsync();
                return new FileDownload
                {
                    ContentType = properties.Value.ContentType,
                    ContentDisposition = properties.Value.ContentDisposition,
                    FileStream = await blob.OpenReadAsync()
                };
            }
            return null;
        }

        public async Task<FileUpload> UploadCABFile(string cabId, string fileName, string directoryName, Stream stream)
        {
            try
            {
                fileName = MakeValidFileName(fileName);
                var blobName = $"{cabId}/{directoryName}/{fileName}";
                var blobClient = _client.GetBlobClient(blobName);
                var blobHeaders = new BlobHttpHeaders
                {
                    ContentType = "application/pdf",
                    ContentDisposition = "attachment; filename=" + fileName
                };

                var result = await blobClient.UploadAsync(stream, blobHeaders);
                if (result.HasValue)
                {
                    return new FileUpload
                    {
                        FileName = fileName,
                        BlobName = blobName,
                        UploadDateTime = DateTime.UtcNow
                    };
                }

                throw new DomainException("File upload failed.");
            }
            catch (Exception ex)
            {
                _loggingService.Log(new LogEntry(ex));
                throw;
            }
        }

        private static string MakeValidFileName(string fileName)
        {
            fileName = fileName.ToLower().Replace(" ", "-");
            // https://stackoverflow.com/a/847251/1762
            var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return $"{System.Text.RegularExpressions.Regex.Replace(fileName, invalidRegStr, string.Empty)}";
        }

        public async Task<bool> DeleteCABSchedule(string blobName)
        {
            try
            {
                var result = await _client.DeleteBlobIfExistsAsync(blobName, DeleteSnapshotsOption.IncludeSnapshots);
                return result.Value;
            }
            catch (Exception ex)
            {
                _loggingService.Log(new LogEntry(ex));
                throw;
            }
        }
    }
}