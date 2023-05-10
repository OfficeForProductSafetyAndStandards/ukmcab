using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.Storage
{
    public interface IFileStorage
    {
        Task<FileUpload> UploadCABFile(string cabId, string FileName, string DirectoryName, Stream stream, string contentType);

        Task<bool> DeleteCABSchedule(string blobName);

        Task<FileDownload> DownloadBlobStream(string blobPath);

        Task<Stream> GetLegacyBlogStream(string blobPath);
    }

    public class FileStorageService : IFileStorage
    {
        private BlobContainerClient _client;
        private BlobContainerClient _legacyClient;
        public FileStorageService(IConfiguration config)
        {
            _client = new BlobContainerClient(config["DataConnectionString"], DataConstants.Storage.Container);
            _client.CreateIfNotExists();
            _legacyClient = new BlobContainerClient(config["DataConnectionString"], DataConstants.Storage.ImportContainer);

        }


        public async Task<Stream> GetLegacyBlogStream(string blobPath)
        {
            var blob = _legacyClient.GetBlobClient(blobPath);
            var blobExists = await blob.ExistsAsync();
            if (blobExists.HasValue && blobExists.Value)
            {
                return await blob.OpenReadAsync();
            }
            return null;
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

        public async Task<FileUpload> UploadCABFile(string cabId, string fileName, string directoryName, Stream stream, string contentType)
        {
            fileName = MakeValidFileName(fileName);
            var blobName = $"{cabId}/{directoryName}/{fileName}";
            var blobClient = _client.GetBlobClient(blobName);
            var blobHeaders = new BlobHttpHeaders
            {
                ContentType = contentType, 
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
            else
            {
                // given this is an unexpected condition, throw a normal exception, let it propagate (UnexpectedExceptionHandlerMiddleware will handle it).
                // The user will receive the "something went wrong+error code" page and it'll be logged in application insights and in the error log
                // Domain exceptions are more for when Ralph states some business rule but then doesn't provide any UX or explanation as to how to tell the user.
                // Domain exceptions are like a fallback business error mechanism, where you want to tell the user a message, but you don't want to log the error because it's a bona fide business rule. 
                throw new Exception("UploadCABFile::File upload failed. UploadAsync returned result.HasValue==false."); 
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
            var result = await _client.DeleteBlobIfExistsAsync(blobName, DeleteSnapshotsOption.IncludeSnapshots);  // let exceptions propagate
            return result.Value;
        }
    }
}