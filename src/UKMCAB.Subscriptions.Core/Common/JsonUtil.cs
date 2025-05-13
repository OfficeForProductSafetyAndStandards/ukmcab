using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKMCAB.Subscriptions.Core.Common;

public static class JsonUtil
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static T? TryDeserialize<T>(string? json) where T : class
    {
        if (json?.Clean() != null)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
            }
            catch { }
        }
        return null;
    }

    public static T? Deserialize<T>(string? json) where T : class
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);
    }

    public static string DecodeChunked(string chunked)
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
