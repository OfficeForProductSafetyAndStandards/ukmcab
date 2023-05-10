namespace UKMCAB.Infrastructure.Logging.Models;

public class LogEntry : Dictionary<string, string>
{
    public DateTimeOffset DateTimeUtc { get; set; } = DateTimeOffset.UtcNow;

    public string? IPAddress
    {
        get => this.GetValueOrDefault(nameof(IPAddress));
        set => this[nameof(IPAddress)] = value;
    }

    public string? Url
    {
        get => this.GetValueOrDefault(nameof(Url));
        set => this[nameof(Url)] = value ?? string.Empty;
    }


    public string? UrlReferrer
    {
        get => this.GetValueOrDefault(nameof(UrlReferrer));
        set => this[nameof(UrlReferrer)] = value ?? string.Empty;
    }

    public string? ExceptionData
    {
        get => this.GetValueOrDefault(nameof(ExceptionData));
        set => this[nameof(ExceptionData)] = value ?? string.Empty;
    }

    public string? Message
    {
        get => this.GetValueOrDefault(nameof(Message));
        set => this[nameof(Message)] = value ?? string.Empty;
    }

    public string? UserAgent
    {
        get => this.GetValueOrDefault(nameof(UserAgent));
        set => this[nameof(UserAgent)] = value ?? string.Empty;
    }

    public string? HttpMethod
    {
        get => this.GetValueOrDefault(nameof(HttpMethod));
        set => this[nameof(HttpMethod)] = value ?? string.Empty;
    }

    public string? UserData
    {
        get => this.GetValueOrDefault(nameof(UserData));
        set => this[nameof(UserData)] = value ?? string.Empty;
    }

    public LogEntry() { }

    public LogEntry(Exception ex)
    {
        ExceptionData = ex.ToString();
        Message = $"{ex.Message} (base:{ex.GetBaseException()?.Message})";
    }
}
