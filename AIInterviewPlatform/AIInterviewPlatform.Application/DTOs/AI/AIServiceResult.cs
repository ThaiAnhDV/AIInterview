namespace AIInterviewPlatform.Application.DTOs.AI;

public class AIServiceError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class AIServiceResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public AIServiceError? Error { get; set; }

    public static AIServiceResult<T> Succeeded(T data)
    {
        return new AIServiceResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static AIServiceResult<T> Failed(string errorCode, string message, T? data = default)
    {
        return new AIServiceResult<T>
        {
            Success = false,
            Data = data,
            Error = new AIServiceError
            {
                ErrorCode = errorCode,
                Message = message
            }
        };
    }
}
