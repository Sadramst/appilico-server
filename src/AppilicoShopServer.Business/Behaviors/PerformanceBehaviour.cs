using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AppilicoShopServer.Business.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that warns when a request takes longer than the threshold.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger;

    /// <summary>Slow request threshold in milliseconds.</summary>
    private const int SlowRequestThresholdMs = 500;

    /// <summary>Initialises the behaviour with the logger.</summary>
    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var timer = Stopwatch.StartNew();
        var response = await next();
        timer.Stop();

        if (timer.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "[Performance] Slow request: {RequestName} ({ElapsedMs} ms)",
                typeof(TRequest).Name,
                timer.ElapsedMilliseconds);
        }

        return response;
    }
}
