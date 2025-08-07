using System.Text.Json.Serialization;

namespace Utilities.Responses;

/// <summary>
/// Represents a standardized API response format that can include success status, data, message, count, and errors.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Errors { get; init; }

    private ApiResponse(bool success, T? data, int? fullCount, string? message, object? errors)
    {
        Success = success;
        Data = data;
        Message = message;
        Count = fullCount;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful API response with the provided data and optional count.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="fullCount">Optional total count of items.</param>
    /// <returns>A new ApiResponse instance indicating success.</returns>
    public static ApiResponse<T> SuccessResponse(T data, int? fullCount) =>
        new(true, data, fullCount, null, null);

    /// <summary>
    /// Creates an error API response with the provided message and optional error details.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <param name="errors">Optional detailed error information.</param>
    /// <returns>A new ApiResponse instance indicating failure.</returns>
    public static ApiResponse<T> ErrorResponse(string message, object? errors = null) =>
        new(false, default, 0, message, errors);
}