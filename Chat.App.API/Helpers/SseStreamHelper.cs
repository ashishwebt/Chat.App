using System.Text;
using System.Text.Json;

namespace Chat.App.API.Helpers;

public sealed class SseResult : IResult
{
    private readonly Func<Stream, CancellationToken, Task> _writeStream;

    public SseResult(Func<Stream, CancellationToken, Task> writeStream,
        IReadOnlyDictionary<string, string>? headers = null)
    {
        _writeStream = writeStream;
        Headers = headers ?? new Dictionary<string, string>();
    }

    public IReadOnlyDictionary<string, string> Headers { get; }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var response = httpContext.Response;
        response.ContentType = SseStreamHelper.MediaType;
        response.Headers.CacheControl = "no-cache";
        response.Headers["X-Accel-Buffering"] = "no";

        foreach (var header in Headers)
        {
            if (!response.Headers.ContainsKey(header.Key))
            {
                response.Headers[header.Key] = header.Value;
            }
        }

        var ct = httpContext.RequestAborted;
        await _writeStream(response.Body, ct);
    }
}

public static class SseStreamHelper
{
    public const string MediaType = "text/event-stream; charset=utf-8";

    public static string FormatJson(object data, string eventName = "message", JsonSerializerOptions? jsonOptions = null)
    {
        var json = JsonSerializer.Serialize(data, jsonOptions);
        return $"event: {eventName}\ndata: {json}\n\n";
    }

    public static string FormatString(string text, string eventName = "message")
    {
        return $"event: {eventName}\ndata: {text}\n\n";
    }

    public static string Done(object data, JsonSerializerOptions? jsonOptions = null)
    {
        return FormatJson(data, "done", jsonOptions);
    }

    public static async Task WriteAsync(Stream stream, string chunk, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(chunk);
        await stream.WriteAsync(bytes.AsMemory(), ct);
        await stream.FlushAsync(ct);
    }
}