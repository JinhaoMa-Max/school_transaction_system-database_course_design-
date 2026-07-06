namespace CampusSecondHand.Api.Common;

public class ApiResponse
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }

    public static ApiResponse Ok(object? data = null, string message = "OK")
    {
        return new ApiResponse
        {
            Code = 200,
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse
        {
            Code = code,
            Success = false,
            Message = message,
            Data = null
        };
    }
}

public class ApiResponse<T>
{
    public int Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "OK")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, int code = 400)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Success = false,
            Message = message,
            Data = default
        };
    }
}
