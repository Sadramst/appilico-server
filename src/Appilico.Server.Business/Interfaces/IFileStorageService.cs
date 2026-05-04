namespace Appilico.Server.Business.Interfaces;

/// <summary>Abstraction for file/blob storage operations.</summary>
public interface IFileStorageService
{
    /// <summary>Uploads a file and returns its public URL.</summary>
    Task<string> UploadAsync(Stream file, string fileName, string contentType);

    /// <summary>Deletes a file by its URL.</summary>
    Task DeleteAsync(string fileUrl);

    /// <summary>Generates a pre-signed (time-limited) download URL.</summary>
    Task<string> GetPresignedUrlAsync(string fileUrl, int expiryMinutes = 30);
}
