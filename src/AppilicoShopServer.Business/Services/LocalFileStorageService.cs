using AppilicoShopServer.Business.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace AppilicoShopServer.Business.Services;

/// <summary>
/// Development file storage — saves files to wwwroot/uploads/ and serves via static files.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    /// <summary>Initialises the local file storage service.</summary>
    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream file, string fileName, string contentType)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(uploadsPath, safeFileName);

        await using var fs = File.Create(filePath);
        await file.CopyToAsync(fs);

        _logger.LogInformation("[LocalStorage] Uploaded {FileName}", safeFileName);
        return $"/uploads/{safeFileName}";
    }

    /// <inheritdoc/>
    public Task DeleteAsync(string fileUrl)
    {
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("[LocalStorage] Deleted {FileName}", fileName);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<string> GetPresignedUrlAsync(string fileUrl, int expiryMinutes = 30)
    {
        // Local files are always accessible — return as-is with a dummy expiry param
        var url = $"{fileUrl}?expires={DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds()}";
        return Task.FromResult(url);
    }
}
