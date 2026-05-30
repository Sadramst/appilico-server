namespace AppilicoShopServer.Domain.Common;

/// <summary>Discriminates why an operation failed.</summary>
public enum ErrorType
{
    /// <summary>No error.</summary>
    None = 0,

    /// <summary>The requested resource was not found.</summary>
    NotFound = 1,

    /// <summary>The caller does not have permission.</summary>
    Forbidden = 2,

    /// <summary>Input validation failed.</summary>
    Validation = 3,

    /// <summary>A uniqueness or state conflict occurred.</summary>
    Conflict = 4,

    /// <summary>General/unclassified error.</summary>
    General = 5
}

/// <summary>
/// Functional result returned from every CQRS handler — no exceptions for business failures.
/// </summary>
/// <typeparam name="T">The value type on success.</typeparam>
public record Result<T>
{
    /// <summary>Gets whether the operation succeeded.</summary>
    public bool IsSuccess { get; init; }

    /// <summary>Gets the success value (null on failure).</summary>
    public T? Value { get; init; }

    /// <summary>Gets the error message (null on success).</summary>
    public string? Error { get; init; }

    /// <summary>Gets the error type.</summary>
    public ErrorType ErrorType { get; init; }

    private Result() { }

    /// <summary>Creates a successful result.</summary>
    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, ErrorType = ErrorType.None };

    /// <summary>Creates a failure result.</summary>
    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.General) =>
        new() { IsSuccess = false, Error = error, ErrorType = errorType };

    /// <summary>Creates a NotFound failure.</summary>
    public static Result<T> NotFound(string error) => Failure(error, ErrorType.NotFound);

    /// <summary>Creates a Forbidden failure.</summary>
    public static Result<T> Forbidden(string error) => Failure(error, ErrorType.Forbidden);

    /// <summary>Creates a Validation failure.</summary>
    public static Result<T> ValidationError(string error) => Failure(error, ErrorType.Validation);

    /// <summary>Creates a Conflict failure.</summary>
    public static Result<T> Conflict(string error) => Failure(error, ErrorType.Conflict);
}
