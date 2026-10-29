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

                    var (status, title, detail) = exception switch
                    {
                        NotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found" , exception.Message),
                        BusinessException => ((int)HttpStatusCode.UnprocessableEntity, "Business rule violation" , exception.Message),
                        InvalidInputException => ((int)HttpStatusCode.BadRequest, "Invalid input",exception.Message),
                        _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred")
                    };
                    context.Response.StatusCode = status;
                    var stackTrace = env.IsDevelopment() ? exception.StackTrace : null;
                    var errorResponse = new ApiProblemDetails(
                        context.TraceIdentifier,
                        stackTrace
                    )
                    {
                        Status = status,
                        Title = title,
                        Detail = detail
                    };

                    await context.Response.WriteAsync(errorResponse.ToString());
                }
            });
        });
    }
}