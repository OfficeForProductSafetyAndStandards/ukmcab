using UKMCAB.Data.Models;

namespace UKMCAB.Data.Storage
{
    public interface IFileStorage
    {
        Task<FileUpload> UploadCABFile(string cabId, string label, string fileName, string directoryName, Stream stream, string contentType);

        Task<bool> DeleteCABSchedule(string blobName);

        Task<FileDownload> DownloadBlobStream(string blobPath);
    }
}