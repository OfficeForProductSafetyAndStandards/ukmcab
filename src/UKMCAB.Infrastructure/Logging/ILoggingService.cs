using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKMCAB.Common;

namespace UKMCAB.Infrastructure.Logging;
internal class QueuedLogEntry
{
    public string PartitionKey { get; } = DateTime.UtcNow.ToString("yyyyMMdd");
    public string RowKey { get; } = Guid.NewGuid().ToString("N");
    public LogEntry LogEntry { get; set; }
    public string ReferenceId => string.Concat(RowKey, PartitionKey);
    public QueuedLogEntry(LogEntry logEntry) => LogEntry = logEntry;
}

public class LogEntry : Dictionary<string, string>
{
    public DateTimeOffset DateTimeUtc { get; set; } = DateTimeOffset.UtcNow;

    public string IPAddress
    {
        get => this[nameof(IPAddress)];
        set => this[nameof(IPAddress)] = value;
    }

    public string Url
    {
        get => this[nameof(Url)];
        set => this[nameof(Url)] = value;
    }


    public string UrlReferrer
    {
        get => this[nameof(UrlReferrer)];
        set => this[nameof(UrlReferrer)] = value;
    }

    public string ExceptionData
    {
        get => this[nameof(ExceptionData)];
        set => this[nameof(ExceptionData)] = value;
    }

    public string Message
    {
        get => this[nameof(Message)];
        set => this[nameof(Message)] = value;
    }

    public string UserAgent
    {
        get => this[nameof(UserAgent)];
        set => this[nameof(UserAgent)] = value;
    }

    public string HttpMethod
    {
        get => this[nameof(HttpMethod)];
        set => this[nameof(HttpMethod)] = value;
    }

    public string UserData
    {
        get => this[nameof(UserData)];
        set => this[nameof(UserData)] = value;
    }

    public LogEntry() { }

    public LogEntry(Exception ex)
    {
        ExceptionData = ex.ToString();
        Message = $"{ex.Message} (base:{ex.GetBaseException()?.Message})";
    }
}


public interface ILoggingService
{
    string TableName { get; }
    int Count { get; }
    Task FlushAsync();
    string Log(LogEntry entry);
}
