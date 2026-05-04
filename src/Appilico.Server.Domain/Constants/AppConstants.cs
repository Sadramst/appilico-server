namespace Appilico.Server.Domain.Constants;

/// <summary>
/// Application-wide constants to avoid magic strings.
/// </summary>
public static class AppConstants
{
    /// <summary>Role name constants.</summary>
    public static class Roles
    {
        /// <summary>SuperAdmin role — full system access.</summary>
        public const string SuperAdmin = "SuperAdmin";
        /// <summary>Admin role.</summary>
        public const string Admin = "Admin";
        /// <summary>Manager role.</summary>
        public const string Manager = "Manager";
        /// <summary>Customer role.</summary>
        public const string Customer = "Customer";
    }

    /// <summary>Pagination defaults.</summary>
    public static class Pagination
    {
        /// <summary>Default page number.</summary>
        public const int DefaultPage = 1;
        /// <summary>Default page size.</summary>
        public const int DefaultPageSize = 10;
        /// <summary>Maximum page size.</summary>
        public const int MaxPageSize = 50;
    }

    /// <summary>Image upload constraints.</summary>
    public static class Images
    {
        /// <summary>Maximum file size in bytes (5 MB).</summary>
        public const long MaxFileSize = 5 * 1024 * 1024;
        /// <summary>Allowed image content types.</summary>
        public static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        /// <summary>Allowed image extensions.</summary>
        public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        /// <summary>Upload directory name.</summary>
        public const string UploadDirectory = "uploads";
    }

    /// <summary>JWT configuration constants.</summary>
    public static class Jwt
    {
        /// <summary>Access token expiry in minutes.</summary>
        public const int AccessTokenExpiryMinutes = 15;
        /// <summary>Refresh token expiry in days.</summary>
        public const int RefreshTokenExpiryDays = 7;
    }

    /// <summary>Rate limiting constants.</summary>
    public static class RateLimiting
    {
        /// <summary>Auth endpoint rate limit per minute.</summary>
        public const int AuthLimitPerMinute = 5;
        /// <summary>General endpoint rate limit per minute.</summary>
        public const int GeneralLimitPerMinute = 100;
    }
}
