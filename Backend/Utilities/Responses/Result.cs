using System.Text.Json.Serialization;

namespace Utilities.Responses;

/// <summary>
/// Base result class for operations that don't return data.
/// Contains common properties for success status, message, count, and errors.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Optional message describing the result of the operation.
    /// Ignored in JSON serialization when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    /// <summary>
    /// Optional count of items (useful for pagination or batch operations).
    /// Ignored in JSON serialization when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; init; }

    /// <summary>
    /// Optional error details or validation errors.
    /// Ignored in JSON serialization when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Errors { get; init; }

    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="success">Whether the operation was successful.</param>
    /// <param name="message">Optional message describing the result.</param>
    /// <param name="count">Optional count of items.</param>
    /// <param name="errors">Optional error details.</param>
    protected Result(bool success, string? message = null, int? count = null, object? errors = null)
    {
        Success = success;
        Message = message;
        Count = count;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful result without data.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <param name="count">Optional count of affected items.</param>
    /// <returns>A new Result instance indicating success.</returns>
    public static Result SuccessResult(string? message = null, int? count = null) =>
        new(true, message, count);

    /// <summary>
    /// Creates an error result without data.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <param name="errors">Optional detailed error information.</param>
    /// <returns>A new Result instance indicating failure.</returns>
    public static Result ErrorResult(string message, object? errors = null) =>
        new(false, message, null, errors);
}

/// <summary>
/// Generic result class for operations that return data.
/// Inherits from Result and adds a Data property for the returned value.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// The data returned by the operation.
    /// Ignored in JSON serialization when null.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    /// <summary>
    /// Initializes a new instance of the Result{T} class.
    /// </summary>
    /// <param name="success">Whether the operation was successful.</param>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">Optional message describing the result.</param>
    /// <param name="count">Optional count of items.</param>
    /// <param name="errors">Optional error details.</param>
    private Result(bool success, T? data, string? message = null, int? count = null, object? errors = null)
        : base(success, message, count, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a successful result with the provided data.
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="message">Optional success message.</param>
    /// <param name="count">Optional total count of items (useful for pagination).</param>
    /// <returns>A new Result{T} instance indicating success.</returns>
    public static Result<T> SuccessResult(T data, string? message = null, int? count = null) =>
        new(true, data, message, count);

    /// <summary>
    /// Creates an error result without data.
    /// </summary>
    /// <param name="message">The error message to include.</param>
    /// <param name="errors">Optional detailed error information.</param>
    /// <returns>A new Result{T} instance indicating failure.</returns>
    public static Result<T> ErrorResult(string message, object? errors = null) =>
        new(false, default, message, null, errors);

    /// <summary>
    /// Creates a successful result with data and count (useful for paginated results).
    /// </summary>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="totalCount">The total count of items.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A new Result{T} instance indicating success with count.</returns>
    public static Result<T> SuccessWithCount(T data, int totalCount, string? message = null) =>
        new(true, data, message, totalCount);

    /// <summary>
    /// Converts a non-generic Result to a generic Result{T} with default data.
    /// </summary>
    /// <param name="result">The base result to convert.</param>
    /// <returns>A new Result{T} instance with the same success status and properties.</returns>
    public static Result<T> FromResult(Result result) =>
        result.Success 
            ? new(true, default, result.Message, result.Count, result.Errors)
            : new(false, default, result.Message, result.Count, result.Errors);
}
