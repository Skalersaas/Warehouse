using System.Text.Json;
using Utilities.Responses;

namespace Api.Middleware;

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

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = ApiResponseFactory.InternalServerError();

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
