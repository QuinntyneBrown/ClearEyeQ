using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearEyeQ.SharedKernel.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution, elapsed time,
/// and success/failure status for observability and diagnostics.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        logger.LogInformation(
            "Handling {RequestName} ({RequestId})",
            requestName, requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation(
                "Handled {RequestName} ({RequestId}) in {ElapsedMs}ms — Succeeded",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Handled {RequestName} ({RequestId}) in {ElapsedMs}ms — Failed: {ErrorMessage}",
                requestName, requestId, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
