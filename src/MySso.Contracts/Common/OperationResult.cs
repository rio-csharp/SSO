namespace MySso.Contracts.Common;

public sealed record OperationResult(bool Succeeded, string? ErrorCode = null, string? ErrorMessage = null)
{
    public static OperationResult Success() => new(true);

    public static OperationResult Failure(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}

public sealed record OperationResult<T>(bool Succeeded, T? Value = default, string? ErrorCode = null, string? ErrorMessage = null)
{
    public static OperationResult<T> Success(T value) => new(true, value);

    public static OperationResult<T> Failure(string errorCode, string errorMessage) => new(false, default, errorCode, errorMessage);
}