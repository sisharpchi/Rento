using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Rento.Shared;

/// <summary>
/// Non-generic result for operations that do not return a value.
/// </summary>
public class ResponseResult
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorCode { get; init; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => string.IsNullOrEmpty(Error);

    public static ResponseResult CreateSuccess() => new();

    public static ResponseResult CreateError(string error, int errorCode = 0) =>
        new() { Error = error, ErrorCode = errorCode };
}

/// <summary>
/// Result for operations that return a value of type <typeparamref name="T"/>.
/// </summary>
public class ResponseResult<T>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Value { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorCode { get; init; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => string.IsNullOrEmpty(Error);

    public static ResponseResult<T> CreateSuccess(T value) =>
        new() { Value = value };

    public static ResponseResult<T> CreateError(string error, int errorCode = 0) =>
        new() { Error = error, ErrorCode = errorCode };
}

/// <summary>
/// Result for operations that return a value and may return structured error data of type <typeparamref name="TError"/>.
/// </summary>
public class ResponseResult<T, TError>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Value { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorCode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public TError? ErrorData { get; init; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Success => string.IsNullOrEmpty(Error);

    public static ResponseResult<T, TError> CreateSuccess(T value) =>
        new() { Value = value };

    public static ResponseResult<T, TError> CreateError(string error, int errorCode = 0, TError? errorData = default) =>
        new() { Error = error, ErrorCode = errorCode, ErrorData = errorData };
}
