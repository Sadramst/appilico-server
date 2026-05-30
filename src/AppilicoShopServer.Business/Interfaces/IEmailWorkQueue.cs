namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Represents background email work executed outside the request path.</summary>
public delegate Task EmailWorkItem(IEmailService emailService, CancellationToken cancellationToken);

/// <summary>Queues non-critical email work for background processing.</summary>
public interface IEmailWorkQueue
{
    /// <summary>Adds email work to the queue.</summary>
    ValueTask QueueAsync(EmailWorkItem workItem, CancellationToken cancellationToken = default);

    /// <summary>Reads queued email work items.</summary>
    IAsyncEnumerable<EmailWorkItem> ReadAllAsync(CancellationToken cancellationToken);
}