namespace Appilico.Server.Business.Interfaces;

/// <summary>Email sending service.</summary>
public interface IEmailService
{
    /// <summary>Sends a password reset email.</summary>
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
}
