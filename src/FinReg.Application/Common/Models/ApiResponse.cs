namespace FinReg.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<string>? ValidationErrors { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
    public static ApiResponse<T> ValidationFail(IReadOnlyList<string> errors) =>
        new() { Success = false, ValidationErrors = errors };
}
