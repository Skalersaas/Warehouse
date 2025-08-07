using System.Text.Json;

namespace API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> _logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception e)
    {
        _logger.LogError("[Unhandled Exception] {e}", e);

        if (context.Response.HasStarted)
            return;

        context.Response.Clear(); // recommended to avoid mixed content
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = ResponseGenerator.InternalServerError();

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
