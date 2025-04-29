using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using UKMCAB.Data.Models;

namespace UKMCAB.Data.Storage;

public class AwsFileStorageService : IFileStorage
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public AwsFileStorageService(IConfiguration config)
    {
        _bucketName = config["Aws:BucketName"] ?? throw new ArgumentNullException("Aws:BucketName");
        var serviceUrl = config["Aws:ServiceUrl"]; // e.g., http://localhost:4566 for LocalStack

        var s3Config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true // Required for LocalStack
        };

        _s3Client = new AmazonS3Client(
            config["Aws:AccessKey"],
            config["Aws:SecretKey"],
            s3Config
        );
    }
    public async Task<bool> DeleteCABSchedule(string blobName)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = blobName
        };

        var response = await _s3Client.DeleteObjectAsync(request);
        return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
    }

    public async Task<FileDownload> DownloadBlobStream(string blobPath)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = blobPath
        };

        var response = await _s3Client.GetObjectAsync(request);

        if (response is null) return null;

        return new  FileDownload
        {
            ContentType = response.Headers.ContentType,
            ContentDisposition = response.Headers.ContentDisposition,
            FileStream = response.ResponseStream
        };
    }

    public async Task<FileUpload> UploadCABFile(string cabId, string label, string fileName, string directoryName, Stream stream, string contentType)
    {
        var blobName = $"{cabId}/{directoryName}/{fileName}";

        await _s3Client.EnsureBucketExistsAsync(_bucketName);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = blobName,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);

        return new FileUpload
        {
            Id = Guid.NewGuid(),
            Label = label,
            FileName = fileName,
            BlobName = blobName,
            UploadDateTime = DateTime.UtcNow
        };
    }

}
