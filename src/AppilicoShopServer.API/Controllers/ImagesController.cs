using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.Interfaces;

namespace AppilicoShopServer.API.Controllers;

/// <summary>
/// Controller for image upload operations.
/// </summary>
[Authorize(Roles = "Admin")]
public class ImagesController : BaseApiController
{
    private readonly IImageService _imageService;

    public ImagesController(IImageService imageService)
    {
        _imageService = imageService;
    }

    /// <summary>Uploads an image to the specified folder.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "general")
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var result = await _imageService.UploadImageAsync(file, folder);
        return Ok(result);
    }

    /// <summary>Deletes an image by its Cloudinary public ID.</summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return BadRequest("Public ID is required.");

        var success = await _imageService.DeleteImageAsync(publicId);
        return success ? Ok(new { message = "Image deleted successfully." }) : NotFound("Image not found.");
    }
}
