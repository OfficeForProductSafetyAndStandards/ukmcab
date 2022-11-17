using UKMCAB.Common;
using UKMCAB.Infrastructure.Logging.Models;

namespace UKMCAB.Infrastructure.Logging;

public interface ILoggingService
{
    int Count { get; }
    QueuedLogEntry[] FlushErrors { get; }
    QueuedLogEntry[] Snapshot { get; }

    Task FlushAsync(CancellationToken cancellationToken);
    string Log(LogEntry entry);
}

public class LoggingService : ILoggingService
{
    private readonly FixedSizedConcurrentQueue<QueuedLogEntry> _q;
    private readonly FixedSizedConcurrentQueue<QueuedLogEntry> _flushErrors;
    private readonly ILoggingRepository _loggingRepository;

    public int Count => _q.Count;

    public QueuedLogEntry[] FlushErrors => _flushErrors.ToArray();

    public QueuedLogEntry[] Snapshot => _q.TakeLast(10).ToArray();

    public LoggingService(ILoggingRepository loggingRepository)
    {
        _q = new FixedSizedConcurrentQueue<QueuedLogEntry>(50);
        _flushErrors = new FixedSizedConcurrentQueue<QueuedLogEntry>(10);
        _loggingRepository = loggingRepository;
    }

    public string Log(LogEntry entry)
    {
        var queueItem = new QueuedLogEntry(entry);
        _q.Enqueue(queueItem);
        return queueItem.ReferenceId;
    }

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        var buffer = _q.DequeueAll();
        if (buffer.Any())
        {
            try
            {
                await _loggingRepository.SaveAsync(buffer, cancellationToken);
                _flushErrors.Clear();
            }
            catch (Exception ex)
            {
                _q.EnqueueAll(buffer);
                _flushErrors.Enqueue(new QueuedLogEntry(new LogEntry(ex)));
            }
        }
    }
}
