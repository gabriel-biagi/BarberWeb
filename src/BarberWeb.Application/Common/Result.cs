namespace BarberWeb.Application.Common;

public class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    public Result(T? value, bool isSuccess, string errorMessage)
    {
        Value = value;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);

    public static Result<T> Success() => new Result<T>(default!, true, string.Empty);
    
    public static Result<T> Failure(string errorMessage) => new Result<T>(default, false, errorMessage);
}