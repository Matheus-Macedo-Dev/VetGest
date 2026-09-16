namespace VetGest.Web.Services.Api;

public sealed record ApiCallResult<T>(bool Succeeded, T? Data, string? Error, bool IsOffline = false,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null, bool IsUnauthorized = false,
    bool IsNotFound = false, bool IsConflict = false)
{
    public static ApiCallResult<T> Success(T data) => new(true, data, null);
    public static ApiCallResult<T> Failure(string error, bool isOffline = false,
        IReadOnlyDictionary<string, string[]>? validationErrors = null, bool isUnauthorized = false,
        bool isNotFound = false, bool isConflict = false) =>
        new(false, default, error, isOffline, validationErrors, isUnauthorized, isNotFound, isConflict);
}