using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Appilico.Server.Business.Services;

/// <summary>
/// No-op email service used in Development — all sends are logged to ILogger instead of sent.
/// </summary>
public class NullEmailService : IEmailService
{
    private readonly ILogger<NullEmailService> _logger;

    /// <summary>Initialises the null email service.</summary>
    public NullEmailService(ILogger<NullEmailService> logger) => _logger = logger;

    /// <inheritdoc/>
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        _logger.LogInformation("[NullEmail] SendPasswordResetEmail → {Email} | token={Token}", toEmail, resetToken);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendWelcomeEmailAsync(string email, string firstName)
    {
        _logger.LogInformation("[NullEmail] SendWelcomeEmail → {Email} | firstName={FirstName}", email, firstName);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendWaitlistConfirmationAsync(string email, int position)
    {
        _logger.LogInformation("[NullEmail] SendWaitlistConfirmation → {Email} | position={Position}", email, position);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendPasswordResetAsync(string email, string token, string resetUrl)
    {
        _logger.LogInformation("[NullEmail] SendPasswordReset → {Email} | resetUrl={Url}", email, resetUrl);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendContactNotificationAsync(ContactMessage msg)
    {
        _logger.LogInformation("[NullEmail] SendContactNotification | from={Email}", msg.Email);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendContactAutoReplyAsync(string email, string name)
    {
        _logger.LogInformation("[NullEmail] SendContactAutoReply → {Email}", email);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SendOrderConfirmationAsync(string email, Order order)
    {
        _logger.LogInformation("[NullEmail] SendOrderConfirmation → {Email} | order={OrderNumber}", email, order.OrderNumber);
        return Task.CompletedTask;
    }
}
