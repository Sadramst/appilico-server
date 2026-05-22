namespace Appilico.Server.Business.Options;

/// <summary>SMTP email configuration bound from the Email section.</summary>
public sealed class EmailOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Email";

    /// <summary>Whether SMTP-backed email sending is intentionally enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>SMTP host.</summary>
    public string? SmtpHost { get; set; }

    /// <summary>SMTP port.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>SMTP username.</summary>
    public string? SmtpUser { get; set; }

    /// <summary>SMTP password or app password.</summary>
    public string? SmtpPass { get; set; }

    /// <summary>Default from address.</summary>
    public string? FromEmail { get; set; }

    /// <summary>Default from display name.</summary>
    public string? FromName { get; set; }

    /// <summary>Client base URL used in outbound links.</summary>
    public string? ClientBaseUrl { get; set; }

    /// <summary>Internal notification recipient for contact messages.</summary>
    public string? NotifyEmail { get; set; }

    /// <summary>Maximum queued email work items held in memory.</summary>
    public int QueueCapacity { get; set; } = 100;

    /// <summary>Checks whether required SMTP settings are present when enabled.</summary>
    public bool HasRequiredSettings =>
        !Enabled ||
        (!IsPlaceholder(SmtpHost)
         && !IsPlaceholder(SmtpUser)
         && !IsPlaceholder(SmtpPass)
         && !string.IsNullOrWhiteSpace(FromEmail)
         && QueueCapacity > 0);

    private static bool IsPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Contains("will-be-overridden", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
}