using MediatR;
using Microsoft.Extensions.Logging;

namespace Appilico.Server.Business.Behaviors;

/// <summary>
/// MediatR pipeline behaviour that logs every request/response with timing.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>Initialises the behaviour with the logger.</summary>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[MediatR] Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            _logger.LogInformation("[MediatR] Handled {RequestName}", requestName);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MediatR] Error handling {RequestName}", requestName);
            throw;
        }
    }
}
