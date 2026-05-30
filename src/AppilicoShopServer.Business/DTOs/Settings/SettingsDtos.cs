namespace AppilicoShopServer.Business.DTOs.Settings;

/// <summary>DTO for app setting.</summary>
public class AppSettingDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the key.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Gets or sets the value.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Gets or sets the group.</summary>
    public string? Group { get; set; }
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
}

/// <summary>DTO for updating settings.</summary>
public class UpdateSettingsRequest
{
    /// <summary>Gets or sets the settings to update.</summary>
    public List<SettingItem> Settings { get; set; } = new();
}

/// <summary>A single setting item.</summary>
public class SettingItem
{
    /// <summary>Gets or sets the key.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Gets or sets the value.</summary>
    public string Value { get; set; } = string.Empty;
}
