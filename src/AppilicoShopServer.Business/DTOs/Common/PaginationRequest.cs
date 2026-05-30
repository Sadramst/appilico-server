namespace AppilicoShopServer.Business.DTOs.Common;

/// <summary>Shared pagination normalization helpers for service/query boundaries.</summary>
public static class PaginationRequest
{
    /// <summary>Default maximum page size for public list endpoints.</summary>
    public const int DefaultMaxPageSize = 50;

    /// <summary>Normalizes page and page size values to safe bounds.</summary>
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = DefaultMaxPageSize)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, maxPageSize);
        return (normalizedPage, normalizedPageSize);
    }
}