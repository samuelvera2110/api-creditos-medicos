namespace HealthCare.Shared.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }
    public List<string>? Errors { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private ApiResponse() { }

    public static ApiResponse<T> Ok(T data, string message) =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Ok(string message) =>
        new() { Success = true, Message = message };

    public static ApiResponse<T> Error(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
    
}