namespace CleanCut.Domain.Common;

/// <summary>
/// Represents the result of a domain operation
/// </summary>
/// <typeparam name="T">The type of the result value</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string Error { get; private set; } = string.Empty;
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static Result<T> Failure(string error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Represents the result of a domain operation without a return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public string Error { get; private set; } = string.Empty;
    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}
