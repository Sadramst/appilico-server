using System.Threading.Channels;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Appilico.Server.Business.Services;

/// <summary>Bounded in-process queue for non-critical email work.</summary>
public sealed class BackgroundEmailQueue : IEmailWorkQueue
{
    private readonly Channel<EmailWorkItem> _queue;

    /// <summary>Initialises the queue.</summary>
    public BackgroundEmailQueue(IOptions<EmailOptions> options)
    {
        var capacity = Math.Max(1, options.Value.QueueCapacity);
        _queue = Channel.CreateBounded<EmailWorkItem>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    /// <inheritdoc/>
    public ValueTask QueueAsync(EmailWorkItem workItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        return _queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<EmailWorkItem> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}

/// <summary>Executes queued email work using scoped email services.</summary>
public sealed class QueuedEmailHostedService : BackgroundService
{
    private readonly IEmailWorkQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QueuedEmailHostedService> _logger;

    /// <summary>Initialises the hosted service.</summary>
    public QueuedEmailHostedService(
        IEmailWorkQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<QueuedEmailHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await workItem(emailService, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Queued email work item failed");
            }
        }
    }
}