using System.Text.RegularExpressions;
using AppilicoShopServer.Business.Exceptions;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppilicoShopServer.Business.Services;

/// <summary>Azure Blob Storage file service using private containers and SAS URLs.</summary>
public class AzureBlobStorageService : IFileStorageService
{
    private static readonly Regex UnsafeFileNameCharacters = new("[^a-zA-Z0-9._-]", RegexOptions.Compiled);
    private readonly AzureStorageOptions _options;
    private readonly ILogger<AzureBlobStorageService> _logger;

    /// <summary>Initialises the Azure Blob Storage service.</summary>
    public AzureBlobStorageService(IOptions<AzureStorageOptions> options, ILogger<AzureBlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream file, string fileName, string contentType)
    {
        EnsureConfigured();
        ValidateUpload(file, fileName, contentType);

        var blobName = BuildBlobName(fileName);
        await using var uploadStream = await PrepareUploadStreamAsync(file);

        try
        {
            var container = CreateContainerClient();
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            var blob = container.GetBlobClient(blobName);
            await blob.UploadAsync(uploadStream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Metadata = new Dictionary<string, string>
                {
                    ["originalFileName"] = Path.GetFileName(fileName)
                }
            });

            _logger.LogInformation("Azure Blob upload completed. BlobName={BlobName} ContentType={ContentType}", blobName, contentType);
            return blob.Uri.ToString();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob upload failed. Status={Status} ErrorCode={ErrorCode}", ex.Status, ex.ErrorCode);
            throw new StorageProviderException("File storage provider could not upload the file.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string fileUrl)
    {
        EnsureConfigured();
        var blobName = ResolveBlobName(fileUrl);
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        try
        {
            var blob = CreateContainerClient().GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();
            _logger.LogInformation("Azure Blob delete completed. BlobName={BlobName}", blobName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob delete failed. Status={Status} ErrorCode={ErrorCode}", ex.Status, ex.ErrorCode);
            throw new StorageProviderException("File storage provider could not delete the file.", ex);
        }
    }

    /// <inheritdoc/>
    public Task<string> GetPresignedUrlAsync(string fileUrl, int expiryMinutes = 30)
    {
        EnsureConfigured();
        var blobName = ResolveBlobName(fileUrl);
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("File URL does not reference a valid blob.", nameof(fileUrl));

        var blob = CreateContainerClient().GetBlobClient(blobName);
        if (!blob.CanGenerateSasUri)
            throw new StorageProviderException("File storage is not configured with credentials that can generate secure download URLs.");

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName!,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(Math.Min(expiryMinutes, _options.SasExpiryMinutes))
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(blob.GenerateSasUri(sasBuilder).ToString());
    }

    private void EnsureConfigured()
    {
        if (!_options.Enabled || !_options.HasRequiredSettings)
            throw new StorageProviderException("Azure Blob Storage is not configured for this environment.");
    }

    private void ValidateUpload(Stream file, string fileName, string contentType)
    {
        if (file == Stream.Null || (file.CanSeek && file.Length == 0))
            throw new ArgumentException("File is empty.", nameof(file));

        if (file.CanSeek && file.Length > _options.MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {_options.MaxFileSizeBytes} bytes.", nameof(file));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        if (!_options.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"File content type '{contentType}' is not allowed.", nameof(contentType));
    }

    private async Task<Stream> PrepareUploadStreamAsync(Stream file)
    {
        if (file.CanSeek)
        {
            file.Position = 0;
            return file;
        }

        var memory = new MemoryStream();
        await file.CopyToAsync(memory);
        if (memory.Length > _options.MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {_options.MaxFileSizeBytes} bytes.", nameof(file));

        memory.Position = 0;
        return memory;
    }

    private BlobContainerClient CreateContainerClient()
    {
        return new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
    }

    private static string BuildBlobName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var safeBaseName = UnsafeFileNameCharacters.Replace(baseName, "-").Trim('-');
        if (string.IsNullOrWhiteSpace(safeBaseName))
            safeBaseName = "file";

        return $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{safeBaseName}{extension}";
    }

    private string ResolveBlobName(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return string.Empty;

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return fileUrl.TrimStart('/');

        var path = uri.AbsolutePath.TrimStart('/');
        var containerPrefix = _options.ContainerName!.Trim('/') + "/";
        return path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase)
            ? Uri.UnescapeDataString(path[containerPrefix.Length..])
            : Uri.UnescapeDataString(path);
    }
}
