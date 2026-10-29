namespace BarberWeb.Application.Common;

public class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public ErrorTypes? ErrorType { get; }
    public string ErrorMessage { get; }
    

    private Result(T? value, bool isSuccess, ErrorTypes? errorType, string errorMessage)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T? value) => new Result<T>(value, true, null, string.Empty);
    public static Result<T> Failure(ErrorTypes errorType, string errorMessage) => new Result<T>(default, false, errorType, errorMessage);
}