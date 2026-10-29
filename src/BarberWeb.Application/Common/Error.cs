namespace BarberWeb.Application.Common;

public sealed record Error(ErrorType Type, string Message);
