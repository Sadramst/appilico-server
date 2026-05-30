namespace AppilicoShopServer.Business.Options;

/// <summary>Azure Blob Storage configuration bound from the AzureStorage section.</summary>
public sealed class AzureStorageOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "AzureStorage";

    /// <summary>Whether Azure Blob backed storage is intentionally enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Azure Storage connection string.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Blob container name.</summary>
    public string? ContainerName { get; set; }

    /// <summary>Maximum upload size in bytes.</summary>
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;

    /// <summary>Default SAS URL expiry in minutes.</summary>
    public int SasExpiryMinutes { get; set; } = 30;

    /// <summary>Allowed upload content types.</summary>
    public string[] AllowedContentTypes { get; set; } =
    {
        "application/octet-stream",
        "application/zip",
        "application/x-zip-compressed",
        "application/vnd.ms-powerbi.visual",
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    /// <summary>Checks whether required storage settings are present when enabled.</summary>
    public bool HasRequiredSettings =>
        !Enabled ||
        (!IsPlaceholder(ConnectionString)
         && !string.IsNullOrWhiteSpace(ContainerName)
         && MaxFileSizeBytes > 0
         && SasExpiryMinutes > 0
         && AllowedContentTypes.Length > 0);

    private static bool IsPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Contains("will-be-overridden", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
}