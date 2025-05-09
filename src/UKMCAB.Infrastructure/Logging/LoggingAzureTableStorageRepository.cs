using Azure.Data.Tables;
using UKMCAB.Common;
using UKMCAB.Common.ConnectionStrings;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Infrastructure.Logging;

public interface ILoggingRepository
{
    Task SaveAsync(IEnumerable<QueuedLogEntry> queuedLogEntries, CancellationToken cancellationToken);
}

public class LoggingAzureTableStorageRepository : ILoggingRepository
{
    private readonly TableClient _tableClient;
    private const string TableName = "errorlog";


    public LoggingAzureTableStorageRepository(DataConnectionString azureDataConnectionString)
    {
        //var tableServiceClient = new TableServiceClient(azureDataConnectionString);
        //_tableClient = tableServiceClient.GetTableClient(TableName);
    }

    public async Task SaveAsync(IEnumerable<QueuedLogEntry> queuedLogEntries, CancellationToken cancellationToken)
    {
        //await EnsureAsync();
        //
        //var entities = queuedLogEntries.Select(item => new TableEntity()
        //{
        //    PartitionKey = item.PartitionKey,
        //    RowKey = item.RowKey,
        //    [nameof(item.LogEntry.HttpMethod)] = item.LogEntry.HttpMethod,
        //    [nameof(item.LogEntry.IPAddress)] = item.LogEntry.IPAddress,
        //    [nameof(item.LogEntry.Url)] = item.LogEntry.Url,
        //    [nameof(item.LogEntry.UrlReferrer)] = item.LogEntry.UrlReferrer,
        //    [nameof(item.LogEntry.ExceptionData)] = item.LogEntry.ExceptionData,
        //    [nameof(item.LogEntry.Message)] = item.LogEntry.Message,
        //    [nameof(item.LogEntry.UserAgent)] = item.LogEntry.UserAgent,
        //    [nameof(item.LogEntry.UserData)] = item.LogEntry.UserData,
        //}).ToArray();
        //
        //foreach (var te in entities)
        //{
        //    await _tableClient.AddEntityAsync(te);
        //}
    }

    private async Task EnsureAsync()
        => await Efficiency.DoOnceAsync(nameof(LoggingAzureTableStorageRepository),
            StringExt.Keyify(nameof(_tableClient.CreateIfNotExistsAsync), _tableClient.Name),
            () => _tableClient.CreateIfNotExistsAsync());

}
