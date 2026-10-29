using BarberWeb.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BarberWeb.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToProblem(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("A successful result cannot be converted to a problem response.");

        var error = result.Error;

        var (status, title) = error.Type switch
        {
            ErrorType.InvalidEntry => (StatusCodes.Status400BadRequest, "Invalid input"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.BusinessRule => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            _ => throw new InvalidOperationException($"ErrorType without HTTP mapping: {error.Type}")
        };

        return controller.Problem(detail: error.Message, statusCode: status, title: title);
    }
}
