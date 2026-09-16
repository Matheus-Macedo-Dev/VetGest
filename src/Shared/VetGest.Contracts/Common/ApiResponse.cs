namespace VetGest.Contracts.Common;

/// <summary>
/// Generic response envelope for all API responses.
/// Provides consistent error and validation feedback to clients.
/// </summary>
/// <typeparam name="TData">The response data type.</typeparam>
public class ApiResponse<TData>
{
    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data (null if not successful or no data).
    /// </summary>
    public TData? Data { get; set; }

    /// <summary>
    /// Error message (null if successful).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Validation errors by field (null if successful or not a validation error).
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Create a successful response with data.
    /// </summary>
    public static ApiResponse<TData> Ok(TData data) => new()
    {
        Success = true,
        Data = data
    };

    /// <summary>
    /// Create a successful response with no data.
    /// </summary>
    public static ApiResponse<TData> Ok() => new()
    {
        Success = true
    };

    /// <summary>
    /// Create a failure response with an error message.
    /// </summary>
    public static ApiResponse<TData> Fail(string error) => new()
    {
        Success = false,
        Error = error
    };

    /// <summary>
    /// Create a failure response with validation errors.
    /// </summary>
    public static ApiResponse<TData> ValidationFailed(Dictionary<string, string[]> errors) => new()
    {
        Success = false,
        Error = "Validation failed.",
        ValidationErrors = errors
    };
}

/// <summary>
/// Non-generic API response for endpoints with no return data.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message (null if successful).
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Validation errors by field.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Create a successful response.
    /// </summary>
    public static ApiResponse Ok() => new()
    {
        Success = true
    };

    /// <summary>
    /// Create a failure response with an error message.
    /// </summary>
    public static ApiResponse Fail(string error) => new()
    {
        Success = false,
        Error = error
    };

    /// <summary>
    /// Create a failure response with validation errors.
    /// </summary>
    public static ApiResponse ValidationFailed(Dictionary<string, string[]> errors) => new()
    {
        Success = false,
        Error = "Validation failed.",
        ValidationErrors = errors
    };
}
