using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Image;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.Business.Services;

public class CloudinaryImageService : IImageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryImageService> _logger;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public CloudinaryImageService(Cloudinary cloudinary, ILogger<CloudinaryImageService> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    public async Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder)
    {
        if (file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"appilico/{folder}",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
        }

        return new ImageUploadResultDto
        {
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId,
            ThumbnailUrl = GetThumbnailUrl(result.PublicId),
            Width = result.Width,
            Height = result.Height,
            Format = result.Format,
            Size = result.Bytes
        };
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return false;

        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        return result.Result == "ok";
    }

    public string GetOptimizedUrl(string publicId, int width, int height)
    {
        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation().Width(width).Height(height).Crop("fit").Quality("auto").FetchFormat("auto"))
            .BuildUrl(publicId);
    }

    public string GetThumbnailUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp
            .Transform(new Transformation().Width(200).Height(200).Crop("fill").Gravity("auto").Quality("auto").FetchFormat("auto"))
            .BuildUrl(publicId);
    }
}
