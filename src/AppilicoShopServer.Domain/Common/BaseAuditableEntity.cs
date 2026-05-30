namespace AppilicoShopServer.Domain.Common;

/// <summary>
/// Base entity with audit fields. All entities inherit from this.
/// </summary>
public abstract class BaseAuditableEntity
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the soft delete flag.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the user who created this entity.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>Gets or sets the user who last updated this entity.</summary>
    public string? UpdatedBy { get; set; }
}
