using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.Exceptions;

namespace PolicyService.API.Infrastructure;

public sealed class PolicyExceptionHandler(ILogger<PolicyExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException and not NotFoundException and not ForbiddenException and not ServiceUnavailableException)
        {
            return false;
        }

        logger.LogWarning(exception, "Request failed with a handled application exception.");

        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ForbiddenException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status503ServiceUnavailable
            },
            Title = exception switch
            {
                ValidationException => "Validation failed.",
                NotFoundException => "Resource not found.",
                ForbiddenException => "Access denied.",
                _ => "Service unavailable."
            },
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}