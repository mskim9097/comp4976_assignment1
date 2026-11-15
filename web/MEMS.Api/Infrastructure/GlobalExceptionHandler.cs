using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MEMS.Api.Infrastructure;

// Global exception handling: the real exception goes to the structured log (and the Aspire dashboard);
// the caller only gets a generic problem response with a trace id they can quote to support.
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        logger.LogError(exception, "Unhandled exception for {Method} {Path} (trace {TraceId})",
            httpContext.Request.Method, httpContext.Request.Path, traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Something went wrong",
            Detail = "An unexpected error occurred. Please try again later.",
            Extensions = { ["traceId"] = traceId }
        }, cancellationToken);
        return true;
    }
}
