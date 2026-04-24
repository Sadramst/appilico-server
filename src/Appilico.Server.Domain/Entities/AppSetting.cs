using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents an application setting stored in the database.
/// </summary>
public class AppSetting : BaseAuditableEntity
{
    /// <summary>Gets or sets the setting key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the setting value.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets the setting group.</summary>
    public string? Group { get; set; }

    /// <summary>Gets or sets the setting description.</summary>
    public string? Description { get; set; }
}
