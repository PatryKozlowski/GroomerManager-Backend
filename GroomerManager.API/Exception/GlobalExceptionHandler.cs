using System.Diagnostics;
using GroomerManager.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace GroomerManager.API.Exception;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        System.Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Could not process a request on machine {MachineName}, TraceId: {TraceId}",
            Environment.MachineName,
            traceId);

        var (statusCode, title, errors) = MapException(exception);
        
        var extensions = new Dictionary<string, object?>
        {
            { "traceId", traceId }
        };
        
        if (errors != null)
        {
            extensions.Add("errors", errors);
        }
        
        await Results.Problem(
            title: title,
            statusCode: statusCode,
            extensions: extensions
        ).ExecuteAsync(httpContext);

        return true;
    }
    private static (int StatusCode, string Title, object? Errors) MapException(System.Exception exception)
    {
        return exception switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, exception.Message, null),
            ErrorException => (StatusCodes.Status400BadRequest, exception.Message, null),
            ValidationException validationException => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation failed",
                validationException.Errors.Select(e => new { e.FieldName, e.Error })
            ),
            _ => (StatusCodes.Status500InternalServerError, "We are working on it !", null)
        };
    }
}