namespace BarberWeb.Domain.Exception;

public class ConcurrencyDuplicateException : System.Exception
{
    public ConcurrencyDuplicateException(string message, System.Exception inner)
        : base(message, inner)
    {
    }
}