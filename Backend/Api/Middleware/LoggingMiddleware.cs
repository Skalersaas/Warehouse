using System.Text;
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
        // Skip swagger endpoints
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // Log request
        var requestBody = await LogRequestAsync(context);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await next(context);
            
            // Log response
            await LogResponseAsync(context, responseBodyStream);
            
            // Copy response back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task<string> LogRequestAsync(HttpContext context)
    {
        var requestBody = string.Empty;
        
        try
        {
            // Enable buffering so the request can be read multiple times
            context.Request.EnableBuffering();
            
            // Read the request body
            if (context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
            
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);
                
            requestBody = await reader.ReadToEndAsync();
            
            // Reset the stream position for the next middleware
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            
            // Log the request
            var formatted = FormatJson(requestBody);
            
            logger.LogInformation("=== HTTP REQUEST ===");
            logger.LogInformation("Method: {Method}", context.Request.Method);
            logger.LogInformation("Path: {Path}", context.Request.Path);
            logger.LogInformation("Query: {Query}", context.Request.QueryString);
            logger.LogInformation("Content-Type: {ContentType}", context.Request.ContentType);
            logger.LogInformation("Content-Length: {ContentLength}", context.Request.ContentLength);
            
            if (!string.IsNullOrEmpty(requestBody))
            {
                logger.LogInformation("Request Body:\n{Body}", formatted);
            }
            else
            {
                logger.LogInformation("Request Body: (empty or no body)");
            }
            logger.LogInformation("===================");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log HTTP request");
        }
        
        return requestBody;
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream responseBodyStream)
    {
        try
        {
            var responseBody = string.Empty;
            
            if (responseBodyStream.Length > 0)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(responseBodyStream, Encoding.UTF8, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
            }
            
            var level = context.Response.StatusCode >= 400 ? LogLevel.Error : LogLevel.Information;
            var formatted = FormatJson(responseBody);
            
            logger.Log(level, "=== HTTP RESPONSE ===");
            logger.Log(level, "Status Code: {StatusCode}", context.Response.StatusCode);
            logger.Log(level, "Content-Type: {ContentType}", context.Response.ContentType);
            logger.Log(level, "Content-Length: {ContentLength}", responseBodyStream.Length);
            
            if (!string.IsNullOrEmpty(responseBody))
            {
                logger.Log(level, "Response Body:\n{Body}", formatted);
            }
            else
            {
                logger.Log(level, "Response Body: (empty)");
            }
            logger.Log(level, "====================");
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
            if (string.IsNullOrWhiteSpace(json)) 
                return json;
                
            // Try to parse and format as JSON
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(doc, JsonOptions);
        }
        catch
        {
            // If it's not valid JSON, return as-is
            return json;
        }
    }
}
