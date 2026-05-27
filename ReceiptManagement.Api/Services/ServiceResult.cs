namespace ReceiptManagement.Api.Services;

public class ServiceResult<T>
{
    public bool Success { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public T? Data { get; private init; }
    public object? Errors { get; private init; }
    public int StatusCode { get; private init; }

    public static ServiceResult<T> Ok(T data, string message = "Request completed successfully.")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public static ServiceResult<T> Created(T data, string message = "Resource created successfully.")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public static ServiceResult<T> NoContent(string message = "Resource deleted successfully.")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            StatusCode = StatusCodes.Status204NoContent
        };
    }

    public static ServiceResult<T> Fail(string message, int statusCode, object? errors = null)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = statusCode
        };
    }
}
