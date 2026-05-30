using AppilicoShopServer.Business.DTOs.Image;
using Microsoft.AspNetCore.Http;

namespace AppilicoShopServer.Business.Interfaces;

public interface IImageService
{
    Task<ImageUploadResultDto> UploadImageAsync(IFormFile file, string folder);
    Task<bool> DeleteImageAsync(string publicId);
    string GetOptimizedUrl(string publicId, int width, int height);
    string GetThumbnailUrl(string publicId);
}
