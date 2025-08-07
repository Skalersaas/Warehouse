using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Utilities.Responses;

/// <summary>
/// Provides utility methods for generating standardized HTTP responses with appropriate status codes and data.
/// </summary>
public class ApiResponseFactory
{
    /// <summary>
    /// Creates a successful response with 200 OK status code.
    /// </summary>
    /// <typeparam name="T">The type of data to include in the response.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="fullCount">Optional total count of items.</param>
    /// <returns>An ObjectResult with 200 OK status code.</returns>
    public static ObjectResult Success<T>(T? data = default, int? fullCount = null)
    {
        var response = ApiResponse<T?>.SuccessResponse(data, fullCount);
        return new(response) { StatusCode = StatusCodes.Status200OK };
    }

    /// <summary>
    /// Creates an error response with the specified status code.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="errors">Optional detailed error information.</param>
    /// <returns>An ObjectResult with the specified status code.</returns>
    public static ObjectResult Error(string message, int statusCode, object? errors = null)
    {
        var response = ApiResponse<object>.ErrorResponse(message, errors);
        return new(response) { StatusCode = statusCode };
    }

    /// <summary>
    /// Creates a successful response with 200 OK status code.
    /// </summary>
    /// <typeparam name="T">The type of data to include in the response.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="fullCount">Optional total count of items.</param>
    /// <returns>An ObjectResult with 200 OK status code.</returns>
    public static ObjectResult Ok<T>(T? data, int? fullCount = null)
        => Success(data, fullCount);

    /// <summary>
    /// Creates a 400 Bad Request response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <param name="errors">Optional detailed error information.</param>
    /// <returns>An ObjectResult with 400 Bad Request status code.</returns>
    public static ObjectResult BadRequest(string message = "Bad request", object? errors = null)
        => Error(message, StatusCodes.Status400BadRequest, errors);

    /// <summary>
    /// Creates a 401 Unauthorized response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>An ObjectResult with 401 Unauthorized status code.</returns>
    public static ObjectResult Unauthorized(string message = "Unauthorized")
        => Error(message, StatusCodes.Status401Unauthorized);

    /// <summary>
    /// Creates a 403 Forbidden response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>An ObjectResult with 403 Forbidden status code.</returns>
    public static ObjectResult Forbidden(string message = "Forbidden")
        => Error(message, StatusCodes.Status403Forbidden);

    /// <summary>
    /// Creates a 404 Not Found response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>An ObjectResult with 404 Not Found status code.</returns>
    public static ObjectResult NotFound(string message = "Resource not found")
        => Error(message, StatusCodes.Status404NotFound);

    /// <summary>
    /// Creates a 409 Conflict response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>An ObjectResult with 409 Conflict status code.</returns>
    public static ObjectResult Conflict(string message = "Conflict occurred")
        => Error(message, StatusCodes.Status409Conflict);

    /// <summary>
    /// Creates a 500 Internal Server Error response.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <returns>An ObjectResult with 500 Internal Server Error status code.</returns>
    public static ObjectResult InternalServerError(string message = "Internal server error")
        => Error(message, StatusCodes.Status500InternalServerError);
}