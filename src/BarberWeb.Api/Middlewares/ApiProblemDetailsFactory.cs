using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BarberWeb.Api.Middlewares;

public static class ApiProblemDetailsFactory
{
    public static ProblemDetails Create(
        HttpContext context,
        int status,
        string title,
        string? detail = null)
    {
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        AddCommonProperties(problemDetails, context);
        return problemDetails;
    }

    public static ValidationProblemDetails CreateValidation(
        HttpContext context,
        ModelStateDictionary modelState)
    {
        var problemDetails = new ValidationProblemDetails(modelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid input",
            Detail = "One or more validation errors occurred."
        };

        AddCommonProperties(problemDetails, context);
        return problemDetails;
    }

    private static void AddCommonProperties(ProblemDetails problemDetails, HttpContext context)
    {
        problemDetails.Type = "about:blank";
        problemDetails.Instance = context.Request.Path.Value;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
    }
}
