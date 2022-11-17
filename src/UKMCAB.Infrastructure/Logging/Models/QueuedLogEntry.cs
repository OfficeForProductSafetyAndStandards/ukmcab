namespace UKMCAB.Infrastructure.Logging.Models;

public class QueuedLogEntry
{
    public string PartitionKey { get; } = DateTime.UtcNow.ToString("yyyyMMdd");
    public string RowKey { get; } = Guid.NewGuid().ToString("N");
    public LogEntry LogEntry { get; set; }
    public string ReferenceId => string.Concat(RowKey, PartitionKey);
    public QueuedLogEntry(LogEntry logEntry) => LogEntry = logEntry;
}
