using Appilico.Server.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appilico.Server.Business.Services;

/// <summary>
/// Azure Blob Storage file service.
/// TODO: Wire Azure.Storage.Blobs SDK — using stub implementation that logs to ILogger.
/// </summary>
public class AzureBlobStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureBlobStorageService> _logger;

    /// <summary>Initialises the Azure Blob Storage service.</summary>
    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<string> UploadAsync(Stream file, string fileName, string contentType)
    {
        // TODO: Implement with BlobContainerClient.UploadBlobAsync
        _logger.LogWarning("[AzureBlob] UploadAsync stub called for {FileName}", fileName);
        return Task.FromResult($"https://placeholder.blob.core.windows.net/visuals/{fileName}");
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileUrl)
    {
        // TODO: Implement with BlobClient.DeleteIfExistsAsync
        _logger.LogWarning("[AzureBlob] DeleteAsync stub called for {Url}", fileUrl);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<string> GetPresignedUrlAsync(string fileUrl, int expiryMinutes = 30)
    {
        // TODO: Implement with BlobSasBuilder to generate time-limited SAS URL
        _logger.LogWarning("[AzureBlob] GetPresignedUrlAsync stub called for {Url}", fileUrl);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();
        return Task.FromResult($"{fileUrl}?sv=stub&se={expiresAt}&sig=placeholder");
    }
}
