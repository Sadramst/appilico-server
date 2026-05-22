using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Options;
using Appilico.Server.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Appilico.Server.Business.Services;

/// <summary>SMTP-based email service using System.Net.Mail.</summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var fromEmail = _options.FromEmail ?? _options.SmtpUser;
        var fromName = _options.FromName ?? "Appilico";

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.SmtpUser))
        {
            _logger.LogWarning("[Email] SMTP not configured - would send '{Subject}' to {Email}", subject, toEmail);
            return;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            Credentials = new NetworkCredential(_options.SmtpUser, _options.SmtpPass),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail!, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
        _logger.LogInformation("[Email] Sent '{Subject}' to {Email}", subject, toEmail);
    }

    /// <inheritdoc/>
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var baseUrl = _options.ClientBaseUrl ?? "https://appilico.com";
        var encodedEmail = Uri.EscapeDataString(toEmail);
        var encodedToken = Uri.EscapeDataString(resetToken);
        var resetLink = $"{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";
        await SendPasswordResetAsync(toEmail, resetToken, resetLink);
    }

    /// <inheritdoc/>
    public async Task SendPasswordResetAsync(string email, string token, string resetUrl)
    {
        const string subject = "Reset Your Appilico Password";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>Password Reset Request</h2>
<p>We received a request to reset your Appilico password.</p>
<p><a href=""{resetUrl}"" style=""padding:12px 24px;background:#4F46E5;color:#fff;text-decoration:none;border-radius:6px;"">Reset Password</a></p>
<p>Link: {resetUrl}</p>
<p>Expires in 1 hour. If you didn't request this, ignore this email.</p>
</body></html>";
        await SendAsync(email, subject, html);
    }

    /// <inheritdoc/>
    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        const string subject = "Welcome to Appilico!";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>Welcome, {WebUtility.HtmlEncode(firstName)}!</h2>
<p>Thanks for joining Appilico - Australia's leading mining analytics platform.</p>
<p>You're now on the Free tier. Upgrade anytime to access more Power BI visuals and features.</p>
<p><a href=""https://appilico.com/dashboard"" style=""padding:12px 24px;background:#4F46E5;color:#fff;text-decoration:none;border-radius:6px;"">Go to Dashboard</a></p>
</body></html>";
        await SendAsync(email, subject, html);
    }

    /// <inheritdoc/>
    public async Task SendWaitlistConfirmationAsync(string email, int position)
    {
        const string subject = "You're on the Appilico Waitlist!";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>You're #{position} on the Waitlist</h2>
<p>Thanks for your interest in Appilico. We'll notify you as soon as access is available.</p>
</body></html>";
        await SendAsync(email, subject, html);
    }

    /// <inheritdoc/>
    public async Task SendContactNotificationAsync(ContactMessage msg)
    {
        var notifyEmail = _options.NotifyEmail ?? "info@appilico.com.au";
        var subject = $"[Contact] {msg.Subject ?? msg.Name}";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>New Contact Message</h2>
    <p><strong>From:</strong> {WebUtility.HtmlEncode(msg.Name)} ({WebUtility.HtmlEncode(msg.Email)})</p>
    <p><strong>Company:</strong> {WebUtility.HtmlEncode(msg.Company ?? "-")}</p>
    <p><strong>Subject:</strong> {WebUtility.HtmlEncode(msg.Subject)}</p>
    <p><strong>Project Type:</strong> {WebUtility.HtmlEncode(msg.ProjectType ?? "-")}</p>
    <p><strong>Budget:</strong> {WebUtility.HtmlEncode(msg.BudgetRange ?? "-")}</p>
<hr/>
    <p>{WebUtility.HtmlEncode(msg.Message)}</p>
</body></html>";
        await SendAsync(notifyEmail, subject, html);
    }

    /// <inheritdoc/>
    public async Task SendContactAutoReplyAsync(string email, string name)
    {
        const string subject = "We received your message — Appilico";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>Hi {WebUtility.HtmlEncode(name)}, thanks for reaching out!</h2>
<p>We've received your message and will respond within 24 hours.</p>
<p>- The Appilico Team</p>
</body></html>";
        await SendAsync(email, subject, html);
    }

    /// <inheritdoc/>
    public async Task SendOrderConfirmationAsync(string email, Order order)
    {
        var subject = $"Order Confirmed - {order.OrderNumber}";
        var html = $@"
<!DOCTYPE html><html><body style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
<h2>Order Confirmed</h2>
<p>Your order <strong>{order.OrderNumber}</strong> has been placed.</p>
<p><strong>Total:</strong> ${order.TotalAmount:F2}</p>
<p>We'll send tracking details once your order is shipped.</p>
</body></html>";
        await SendAsync(email, subject, html);
    }
}
