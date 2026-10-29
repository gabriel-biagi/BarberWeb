using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberWeb.Api.Middlewares;

public class ApiProblemDetails : ProblemDetails
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public string? TraceId { get; private set; }
    public string? StackTrace { get; private set; }

    public ApiProblemDetails(string? traceId, string? stackTrace) : base()
    {
        TraceId = traceId;
        StackTrace = stackTrace;
    }
    
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }
}