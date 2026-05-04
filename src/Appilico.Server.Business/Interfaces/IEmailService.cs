using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Email sending service — all implementations must be safe to call in any environment.</summary>
public interface IEmailService
{
    /// <summary>Sends a password reset email.</summary>
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);

    /// <summary>Sends a welcome email after registration.</summary>
    Task SendWelcomeEmailAsync(string email, string firstName);

    /// <summary>Sends a waitlist confirmation email with the subscriber's position.</summary>
    Task SendWaitlistConfirmationAsync(string email, int position);

    /// <summary>Sends a password reset email (spec-compatible overload).</summary>
    Task SendPasswordResetAsync(string email, string token, string resetUrl);

    /// <summary>Sends an internal notification about a new contact message.</summary>
    Task SendContactNotificationAsync(ContactMessage msg);

    /// <summary>Sends an auto-reply to the contact form submitter.</summary>
    Task SendContactAutoReplyAsync(string email, string name);

    /// <summary>Sends an order confirmation email.</summary>
    Task SendOrderConfirmationAsync(string email, Order order);
}
