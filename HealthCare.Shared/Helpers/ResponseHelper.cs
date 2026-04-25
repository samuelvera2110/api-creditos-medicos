using HealthCare.Shared.Wrappers;

namespace HealthCare.Shared.Helpers;

public static class ResponseHelper
{
    public static ApiResponse<T> Ok<T>(T data, string message) 
        => ApiResponse<T>.Ok(data, message);

    public static ApiResponse<T> Ok<T>(string message) 
        => ApiResponse<T>.Ok(message);

    public static ApiResponse<T> Error<T>(string message, List<string>? errors = null) 
        => ApiResponse<T>.Error(message, errors);
}