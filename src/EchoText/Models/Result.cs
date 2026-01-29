namespace EchoText.Models;

/// <summary>
/// Represents the result of an operation that may fail
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The result value if successful, otherwise default
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Error message if failed, otherwise null
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Create a successful result
    /// </summary>
    /// <param name="value">The result value</param>
    /// <returns>A successful Result</returns>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Create a failed result
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>A failed Result</returns>
    public static Result<T> Failure(string error) => new(false, default, error);

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}
