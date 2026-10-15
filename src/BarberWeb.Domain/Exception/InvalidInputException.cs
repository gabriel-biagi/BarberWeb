namespace BarberWeb.Domain.Exception;

public class InvalidInputException : System.Exception
{
    public InvalidInputException(string message) : base(message) { }
}