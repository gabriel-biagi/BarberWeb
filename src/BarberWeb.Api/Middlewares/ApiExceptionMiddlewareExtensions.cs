using System.Net;
using BarberWeb.Domain.Exception;
using Microsoft.AspNetCore.Diagnostics;

namespace BarberWeb.Api.Middlewares;

public static class ApiExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;

                    var (status, message) = exception switch
                    {
                        NotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
                        BusinessException => ((int)HttpStatusCode.UnprocessableEntity, exception.Message),
                        InvalidInputException => ((int)HttpStatusCode.BadRequest, exception.Message),
                        UniqueConstraintViolationException => ((int)HttpStatusCode.Conflict, exception.Message),
                        _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
                    };
                    context.Response.StatusCode = status;
                    var stackTrace = env.IsDevelopment() ? exception.StackTrace : null;
                    var errorResponse = new ErrorDetails(
                        status,
                        message,
                        context.TraceIdentifier,
                        stackTrace
                    );

                    await context.Response.WriteAsync(errorResponse.ToString());
                }
            });
        });
    }
}