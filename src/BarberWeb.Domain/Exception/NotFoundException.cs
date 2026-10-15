namespace BarberWeb.Domain.Exception;

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message) { }
}