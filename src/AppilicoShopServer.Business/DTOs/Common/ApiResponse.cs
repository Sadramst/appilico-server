namespace AppilicoShopServer.Business.DTOs.Common;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Gets or sets whether the request was successful.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the response message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the response data.</summary>
    public T? Data { get; set; }

    /// <summary>Gets or sets pagination info.</summary>
    public PaginationMeta? Pagination { get; set; }

    /// <summary>Gets or sets error details.</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Gets or sets the response timestamp.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a success response.</summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    /// <summary>Creates a success response with pagination.</summary>
    public static ApiResponse<T> SuccessResponse(T data, PaginationMeta pagination, string message = "Success")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data, Pagination = pagination };
    }

    /// <summary>Creates a failure response.</summary>
    public static ApiResponse<T> FailResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors ?? new() };
    }
}

/// <summary>
/// Pagination metadata.
/// </summary>
public class PaginationMeta
{
    /// <summary>Gets or sets the current page.</summary>
    public int CurrentPage { get; set; }

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets or sets the total count.</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets or sets the total pages.</summary>
    public int TotalPages { get; set; }

    /// <summary>Gets whether there is a previous page.</summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>Gets whether there is a next page.</summary>
    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>Creates pagination metadata.</summary>
    public static PaginationMeta Create(int currentPage, int pageSize, int totalCount)
    {
        return new PaginationMeta
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
