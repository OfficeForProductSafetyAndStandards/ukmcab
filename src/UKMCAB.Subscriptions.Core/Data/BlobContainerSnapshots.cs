using Amazon.S3;

namespace UKMCAB.Subscriptions.Core.Data;

public static class S3SnapshotsBucket
{
    public static IAmazonS3 Create(string? accessKeyId, string? secretAccessKey, string region)
    {
        ArgumentNullException.ThrowIfNull(accessKeyId);
        ArgumentNullException.ThrowIfNull(secretAccessKey);

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        return new AmazonS3Client(accessKeyId, secretAccessKey, config);
    }

    public static string GetBucketName()
    {
        return $"{SubscriptionsCoreServicesOptions.BlobContainerPrefix}snapshots";
    }
}