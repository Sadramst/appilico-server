using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>SMTP-based email service.</summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPass"];
        var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
        var fromName = _configuration["Email:FromName"] ?? "Appilico";
        var baseUrl = _configuration["Email:ClientBaseUrl"] ?? "https://appilico.com";

        var encodedEmail = Uri.EscapeDataString(toEmail);
        var encodedToken = Uri.EscapeDataString(resetToken);
        var resetLink = $"{baseUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        var subject = "Reset Your Appilico Password";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
  <h2 style=""color: #333;"">Password Reset Request</h2>
  <p>Hi,</p>
  <p>We received a request to reset your password for your Appilico account.</p>
  <p>
    <a href=""{resetLink}""
       style=""display: inline-block; padding: 12px 24px; background-color: #4F46E5; color: #ffffff;
              text-decoration: none; border-radius: 6px; font-weight: bold;"">
      Reset Password
    </a>
  </p>
  <p>Or copy and paste this link into your browser:</p>
  <p style=""word-break: break-all; color: #4F46E5;"">{resetLink}</p>
  <p>This link will expire shortly. If you didn't request a password reset, you can safely ignore this email.</p>
  <hr style=""border: none; border-top: 1px solid #eee; margin: 20px 0;"">
  <p style=""font-size: 12px; color: #999;"">Appilico &mdash; Your E-Commerce Platform</p>
</body>
</html>";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser))
        {
            _logger.LogWarning("SMTP not configured. Reset link for {Email}: {Link}", toEmail, resetLink);
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
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
        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }
}
