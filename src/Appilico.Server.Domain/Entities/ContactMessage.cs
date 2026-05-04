using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents an inbound contact message.</summary>
public class ContactMessage : BaseAuditableEntity
{
    /// <summary>Gets or sets the sender name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the company.</summary>
    public string? Company { get; set; }

    /// <summary>Gets or sets the subject.</summary>
    public string? Subject { get; set; }

    /// <summary>Gets or sets the message body.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the project type.</summary>
    public string? ProjectType { get; set; }

    /// <summary>Gets or sets the budget range.</summary>
    public string? BudgetRange { get; set; }

    /// <summary>Gets or sets the preferred contact method.</summary>
    public string? PreferredContactMethod { get; set; }

    /// <summary>Gets or sets whether the message has been read.</summary>
    public bool IsRead { get; set; }

    /// <summary>Gets or sets when the message was read.</summary>
    public DateTime? ReadAt { get; set; }
}
