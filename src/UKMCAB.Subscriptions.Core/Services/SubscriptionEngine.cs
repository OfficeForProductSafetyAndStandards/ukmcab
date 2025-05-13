using System.Text;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.Util;
using UKMCAB.Subscriptions.Core.Common;
using UKMCAB.Subscriptions.Core.Data;
using UKMCAB.Subscriptions.Core.Data.Models;
using UKMCAB.Subscriptions.Core.Domain;
using UKMCAB.Subscriptions.Core.Domain.Emails;
using UKMCAB.Subscriptions.Core.Integration.CabService;
using UKMCAB.Subscriptions.Core.Integration.OutboundEmail;

namespace UKMCAB.Subscriptions.Core.Services;


public class SubscriptionEngine : ISubscriptionEngine, IClearable
{
    private readonly SubscriptionsCoreServicesOptions _options;
    private readonly ILogger<SubscriptionEngine> _logger;
    private readonly IOutboundEmailSender _outboundEmailSender;
    private readonly IRepositories _repositories;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICabService _cabService;
    private readonly IEmailTemplatesService _emailTemplatesService;
    private readonly IAmazonS3 _blobs;

    public SubscriptionEngine(
        SubscriptionsCoreServicesOptions options, 
        ILogger<SubscriptionEngine> logger, 
        IOutboundEmailSender outboundEmailSender, 
        IRepositories repositories, 
        IDateTimeProvider dateTimeProvider,
        ICabService cabService, 
        IEmailTemplatesService emailTemplatesService)
    {

        _options = options;
        _logger = logger;
        _outboundEmailSender = outboundEmailSender;
        _repositories = repositories;
        _dateTimeProvider = dateTimeProvider;
        _cabService = cabService;
        _emailTemplatesService = emailTemplatesService;
        var s3Config = new AmazonS3Config
        {
            ServiceURL = options.AwsServiceUrl,
            ForcePathStyle = true
        };
        _blobs = new AmazonS3Client(
            options.AwsAccessKey,
            options.AwsSecretKey,
            s3Config
        );
        _options.EmailTemplates.Validate();
    }

    public enum Result { Notified, Initialised, NoChange, NotDue, Error }

    public class ResultAccumulator
    {
        public int Notified { get; set; }
        public int Initialised { get; set; }
        public int NoChange { get; set; }
        public int NotDue { get; set; }
        public int Errors { get; set; }

        public int Accept(Result result) => result switch
        {
            Result.Initialised => ++Initialised,
            Result.Notified => ++Notified,
            Result.NoChange => ++NoChange,
            Result.NotDue => ++NotDue,
            Result.Error => ++Errors,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Returns whether the engine *can* process subscriptions (whether the uri templates are configured)
    /// </summary>
    /// <returns></returns>
    public bool CanProcess() => _emailTemplatesService.IsConfigured();

    /// <inheritdoc />
    public async Task<ResultAccumulator> ProcessAsync(CancellationToken cancellationToken)
    {
        _emailTemplatesService.AssertIsUriTemplateOptionsConfigured();

        var rv = new ResultAccumulator();

        await EnsureBlobContainerAsync();

        var page = await _repositories.Subscriptions.GetAllAsync();
        await foreach(var subscription in page)
        {
            await ProcessSubscriptionAsync(rv, subscription);

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }

        return rv;
    }

    private async Task ProcessSubscriptionAsync(ResultAccumulator rv, SubscriptionEntity subscription)
    {
        if (subscription.IsInitialised())
        {
            if (subscription.IsDue(_dateTimeProvider))
            {
                rv.Accept(await HandleDueSubscription(subscription));
            }
            else
            {
                rv.Accept(Result.NotDue);
            }
        }
        else
        {
            await InitialiseSubscriptionAsync(subscription);
            rv.Accept(Result.Initialised);
        }
    }

    private Task InitialiseSubscriptionAsync(SubscriptionEntity subscription)=> subscription.SubscriptionType == SubscriptionType.Search 
        ? InitialiseSearchSubscriptionAsync(subscription) 
        : InitialiseCabSubscriptionAsync(subscription);

    private Task<Result> HandleDueSubscription(SubscriptionEntity subscription) => subscription.SubscriptionType == SubscriptionType.Search
        ? HandleDueSearchSubscription(subscription)
        : HandleDueCabSubscription(subscription);

    private async Task InitialiseSearchSubscriptionAsync(SubscriptionEntity subscription)
    {
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Search, $"The subscription type should be '{SubscriptionType.Search}'");
        Guard.IsTrue(subscription.LastThumbprint is null, "The subscription does not need to be initialised.");

        var data = await GetSearchResultDataAsync(subscription.SearchQueryString);

        subscription.LastThumbprint = data.Thumbprint;
        subscription.DueBaseDate = _dateTimeProvider.UtcNow;
        
        //await _blobs.GetBlobClient(subscription.BlobName).UploadAsync(new BinaryData(data.Json), true).ConfigureAwait(false);
        var request = new PutObjectRequest
        {
            BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
            Key = subscription.BlobName, // like "folder/file.json"
            InputStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Json)),
            ContentType = "application/json"
        };
        await _blobs.PutObjectAsync(request).ConfigureAwait(false);

        await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
        await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Initialised subscription with thumbprint '{subscription.LastThumbprint}' and blob '{subscription.BlobName}'").ConfigureAwait(false);

        // Send the "subscribed" notification
        var email = _emailTemplatesService.GetSubscribedSearchEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), subscription.SearchQueryString, subscription.GetSearchSubscriptionTopicName());
        await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
    }

    private async Task InitialiseCabSubscriptionAsync(SubscriptionEntity subscription)
    {
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Cab, $"The subscription type should be '{SubscriptionType.Cab}'");
        Guard.IsTrue(subscription.LastThumbprint is null, "The subscription does not need to be initialised.");
        Guard.IsNotNull(subscription.CabId, $"{nameof(subscription.CabId)} should not be null");
        
        var data = await GetCabDataAsync(subscription.CabId!.Value) ?? throw new Exception("Cab data not found when initialising CAB subscription");

        subscription.LastThumbprint = data.Thumbprint;
        subscription.CabName = data.Name;
        subscription.DueBaseDate = _dateTimeProvider.UtcNow;

        //await _blobs.GetBlobClient(subscription.BlobName).UploadAsync(new BinaryData(data.Json), true).ConfigureAwait(false);
        var request = new PutObjectRequest
        {
            BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
            Key = subscription.BlobName, // like "folder/file.json"
            InputStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Json)),
            ContentType = "application/json"
        };
        await _blobs.PutObjectAsync(request).ConfigureAwait(false);
        await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
        await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Initialised subscription with thumbprint '{subscription.LastThumbprint}'").ConfigureAwait(false);

        // Send the "subscribed" notification
        var email = _emailTemplatesService.GetSubscribedCabEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), subscription.CabId!.Value, subscription.CabName);
        await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
    }

    private async Task<Result> HandleDueSearchSubscription(SubscriptionEntity subscription)
    {
        var rv = Result.NoChange;
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Search, $"The subscription type should be '{SubscriptionType.Search}'");
        Guard.IsTrue(subscription.LastThumbprint is not null, "The subscription needs to be initialised.");

        var data = await GetSearchResultDataAsync(subscription.SearchQueryString);
        
        if (subscription.LastThumbprint.DoesNotEqual(data.Thumbprint, StringComparison.Ordinal)) // search results have changed.
        {
            //var blobClient = _blobs.GetBlobClient(subscription.BlobName);
            //var content = await blobClient.DownloadContentAsync();
            //var previousResults = content.Value.Content.ToObjectFromJson<List<SubscriptionsCoreCabSearchResultModel>>();
            var getRequest = new GetObjectRequest
            {
                BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                Key = subscription.BlobName
            };

            using var response = await _blobs.GetObjectAsync(getRequest).ConfigureAwait(false);
            using var reader = new StreamReader(response.ResponseStream);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            content = DecodeChunked(content);
            var previousResults = JsonSerializer.Deserialize<List<SubscriptionsCoreCabSearchResultModel>>(
                content, 
                new JsonSerializerOptions { WriteIndented = false }
            );

            var changes = new SearchResultsChangesModel(previousResults, data.Results);
            var changesBlobName = string.Concat(Guid.NewGuid(), ".json");
            //await _blobs.GetBlobClient(changesBlobName).UploadAsync(new BinaryData(changes)).ConfigureAwait(false);
            var request = new PutObjectRequest
            {
                BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                Key = subscription.BlobName, // like "folder/file.json"
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Json)),
                ContentType = "application/json"
            };
            await _blobs.PutObjectAsync(request).ConfigureAwait(false);

            var oldThumbprint = subscription.SetThumbprint(data.Thumbprint);
            subscription.DueBaseDate = _dateTimeProvider.UtcNow;

            //await _blobs.GetBlobClient(subscription.BlobName).UploadAsync(new BinaryData(data.Json), true).ConfigureAwait(false);
            var secondRequest = new PutObjectRequest
            {
                BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                Key = subscription.BlobName, // like "folder/file.json"
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Json)),
                ContentType = "application/json"
            };
            await _blobs.PutObjectAsync(secondRequest).ConfigureAwait(false);

            try
            {
                var email = _emailTemplatesService.GetSearchUpdatedEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), subscription.SearchQueryString, changesBlobName, subscription.GetSearchSubscriptionTopicName());
                await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
                await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                    $"Notified subscription: Search updated. (old:{oldThumbprint}, new:{subscription.LastThumbprint})").ConfigureAwait(false);
                rv = Result.Notified;
            }
            catch (Exception ex)
            {
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Failed to notify change on subscription; error: {ex}").ConfigureAwait(false);
                rv = Result.Error;
            }

        }
        return rv;
    }


    private async Task<Result> HandleDueCabSubscription(SubscriptionEntity subscription)
    {
        var rv = Result.NoChange;
        Guard.IsTrue(subscription.SubscriptionType == SubscriptionType.Cab, $"The subscription type should be '{SubscriptionType.Cab}'");
        Guard.IsTrue(subscription.LastThumbprint is not null, "The subscription needs to be initialised.");

        var cabId = subscription.CabId ?? throw new Exception("Cab ID is null");

        var data = await GetCabDataAsync(cabId); // if the CAB is no longer found, just leave the subscription dormant.

        if (data != null && subscription.LastThumbprint.DoesNotEqual(data.Thumbprint, StringComparison.Ordinal)) // search results have changed.
        {
            var old = new
            {
                subscription.LastThumbprint,
                subscription.BlobName,
            };

            subscription.LastThumbprint = data.Thumbprint;
            subscription.DueBaseDate = _dateTimeProvider.UtcNow;
            subscription.CabName = data.Name;

            //await _blobs.GetBlobClient(subscription.BlobName).UploadAsync(new BinaryData(data.Json), true).ConfigureAwait(false);
            var request = new PutObjectRequest
            {
                BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                Key = subscription.BlobName, // like "folder/file.json"
                InputStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Json)),
                ContentType = "application/json"
            };
            await _blobs.PutObjectAsync(request).ConfigureAwait(false);

            try
            {
                var email = _emailTemplatesService.GetCabUpdatedEmailDefinition(subscription.EmailAddress, subscription.GetKeys(), cabId, subscription.CabName!);
                await _outboundEmailSender.SendAsync(email).ConfigureAwait(false);
                await _repositories.Subscriptions.UpsertAsync(subscription).ConfigureAwait(false);
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(),
                    $"Notified subscription: Cab updated  '{subscription.CabName}'. (old:{old.LastThumbprint}; {old.BlobName}, new:{subscription.LastThumbprint}, {subscription.BlobName}) ").ConfigureAwait(false);
                rv = Result.Notified;
            }
            catch (Exception ex)
            {
                await _repositories.Telemetry.TrackAsync(subscription.GetKeys(), $"Failed to notify change on subscription; error: {ex}").ConfigureAwait(false);
                rv = Result.Error;
            }

        }
        return rv;
    }

    record SearchData(string Thumbprint, string Json, List<SubscriptionsCoreCabSearchResultModel> Results);
    record CabData(string Name, string Thumbprint, string Json);

    private async Task<SearchData> GetSearchResultDataAsync(string? searchQueryString)
    {
        var searchResults = (await _cabService.SearchAsync(searchQueryString)) ?? throw new Exception("Search returned null");
        var list = searchResults.Results.OrderBy(x => x.CabId).ToList();
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = false }) ?? throw new Exception("Serializing search results returned null");
        var thumbprint = json.Md5() ?? throw new Exception("MD5 hashing returned null");
        return new(thumbprint, json, list);
    }

    private async Task<CabData?> GetCabDataAsync(Guid id)
    {
        var cab = await _cabService.GetAsync(id);
        if(cab != null)
        {
            var json = JsonSerializer.Serialize(cab, new JsonSerializerOptions { WriteIndented = false }) ?? throw new Exception("Serializing cab returned null");
            var thumbprint = json.Md5() ?? throw new Exception("MD5 hashing returned null");
            return new(cab.Name ?? string.Empty, thumbprint, json);
        }
        else
        {
            return null;
        }
    }

    private async Task EnsureBlobContainerAsync()
    {
        if (Memory.Set(GetType(), nameof(EnsureBlobContainerAsync)))
        {
            //await _blobs.CreateIfNotExistsAsync();

            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_blobs, SubscriptionsCoreServicesOptions.BlobContainerPrefix).ConfigureAwait(false);

            if (!exists)
            {
                var request = new PutBucketRequest
                {
                    BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                    UseClientRegion = true
                };

                await _blobs.PutBucketAsync(request).ConfigureAwait(false);
            }
        }
    }

    async Task IClearable.ClearDataAsync()
    {
        //var pages = _blobs.GetBlobsAsync().AsPages(pageSizeHint: 10);
        //await foreach (var page in pages)
        //{
        //    foreach (var blob in page.Values)
        //    {
        //        await _blobs.DeleteBlobIfExistsAsync(blob.Name).ConfigureAwait(false);
        //    }
        //}
        string? continuationToken = null;

        do
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = SubscriptionsCoreServicesOptions.BlobContainerPrefix,
                MaxKeys = 10,
                ContinuationToken = continuationToken
            };

            var listResponse = await _blobs.ListObjectsV2Async(listRequest).ConfigureAwait(false);

            if (listResponse.S3Objects is not null)
            {
                foreach (var s3Object in listResponse.S3Objects)
                {
                    await _blobs.DeleteObjectAsync(SubscriptionsCoreServicesOptions.BlobContainerPrefix, s3Object.Key).ConfigureAwait(false);
                }
            }

            continuationToken = listResponse.IsTruncated == true ? listResponse.NextContinuationToken : null;

        } while (continuationToken != null);
    }

    private string DecodeChunked(string chunked)
    {
        var reader = new StringReader(chunked);
        var output = new StringBuilder();

        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();
            if (int.TryParse(line, System.Globalization.NumberStyles.HexNumber, null, out var chunkSize))
            {
                if (chunkSize == 0) break;
                char[] buffer = new char[chunkSize];
                reader.ReadBlock(buffer, 0, chunkSize);
                output.Append(buffer);
                reader.ReadLine(); // skip the \r\n
            }
            else
            {
                break;
            }
        }

        return output.ToString();
    }

}
