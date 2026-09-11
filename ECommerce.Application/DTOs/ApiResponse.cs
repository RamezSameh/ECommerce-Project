namespace ECommerce.Application.DTOs;

/// <summary>
/// Standard API envelope returned by every controller action.
/// Guarantees a consistent response shape for success and error cases.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IReadOnlyList<string>? Errors { get; set; }

    private ApiResponse() { }

    public static ApiResponse<T> Success(T data, string message = "Success") =>
        new() { IsSuccess = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { IsSuccess = false, Message = message, Errors = errors };

    public static ApiResponse<T> Fail(IReadOnlyList<string> errors, string message = "Validation failed") =>
        new() { IsSuccess = false, Message = message, Errors = errors };
}