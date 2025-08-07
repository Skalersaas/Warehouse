using System.Text.Json;

namespace Api.Middleware;

public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        context.Request.EnableBuffering();

        var requestBody = await ReadStreamAsync(context.Request.Body);
        LogRequest(context, requestBody);

        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await next(context);
            var body = await ReadStreamAsync(responseBodyStream);
            LogResponse(context, body);

            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        if (stream == null || stream.Length == 0)
            return string.Empty;
        using var reader = new StreamReader(stream, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        stream.Seek(0, SeekOrigin.Begin);
        return content;
    }
    private void LogRequest(HttpContext context, string body)
    {
        try
        {
            var formatted = FormatJson(body);
            logger.LogInformation("HTTP Request: {Method} {Path}\nRequest Body:\n{Body}",
                context.Request.Method, context.Request.Path, formatted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log HTTP request");
        }
    }
    private void LogResponse(HttpContext context, string body)
    {
        try
        {
            var level = context.Response.StatusCode >= 400 ? LogLevel.Error : LogLevel.Information;
            var formatted = FormatJson(body);

            logger.Log(level, "HTTP Response: {StatusCode}\nResponse Body:\n{Body}",
                context.Response.StatusCode, formatted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log HTTP response");
        }
    }


    private static string FormatJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(doc, JsonOptions);
        }
        catch
        {
            return json;
        }
    }
}