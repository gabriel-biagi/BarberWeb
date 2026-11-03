using BarberWeb.Domain.Exception;
using Microsoft.AspNetCore.Diagnostics;

namespace BarberWeb.Api.Middlewares;

public static class ApiExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("BarberWeb.Api.ExceptionHandler");

                var exception = context.Features
                    .Get<IExceptionHandlerFeature>()?.Error;

                var (status, title, detail) = exception switch
                {
                    NotFoundException notFound =>
                        (StatusCodes.Status404NotFound, "Resource not found", notFound.Message),

                    BusinessException business =>
                        (StatusCodes.Status422UnprocessableEntity, "Business rule violation", business.Message),

                    InvalidInputException invalidInput =>
                        (StatusCodes.Status400BadRequest, "Invalid input", invalidInput.Message),

                    _ =>
                        (StatusCodes.Status500InternalServerError, "Internal Server Error",
                            "An unexpected error occurred")
                };

                if (exception is null)
                {
                    logger.LogError("The exception handler was invoked without an exception feature.");
                }
                else if (status >= StatusCodes.Status500InternalServerError)
                {
                    logger.LogError(exception, "An unexpected error occurred while processing the request.");
                }
                else
                {
                    logger.LogWarning(exception, "A handled application exception occurred.");
                }

                context.Response.Clear();
                context.Response.StatusCode = status;

                var problemDetails = ApiProblemDetailsFactory.Create(
                    context,
                    status,
                    title,
                    detail);

                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(
                    problemDetails,
                    cancellationToken: context.RequestAborted);
            });
        });
    }
}