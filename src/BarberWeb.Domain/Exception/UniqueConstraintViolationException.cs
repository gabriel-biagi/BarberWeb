namespace BarberWeb.Domain.Exception;

public class UniqueConstraintViolationException : System.Exception
{
    public UniqueConstraintViolationException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}