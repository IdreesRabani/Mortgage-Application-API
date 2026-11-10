using System.Text.Json.Serialization;

namespace Mortage_Application_API.Logging
{
    public record LogEntry(
        DateTime Timestamp,
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        LogLevel Level,
        string Category,
        string Message,
        string? Exception
    );
}
