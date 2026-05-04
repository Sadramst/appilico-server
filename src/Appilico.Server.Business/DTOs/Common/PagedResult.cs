namespace Appilico.Server.Business.DTOs.Common;

/// <summary>Paginated result container used by new CQRS handlers.</summary>
/// <typeparam name="T">The item type.</typeparam>
public class PagedResult<T>
{
    /// <summary>Gets or sets the items on this page.</summary>
    public List<T> Items { get; set; } = new();

    /// <summary>Gets or sets the current page (1-based).</summary>
    public int Page { get; set; }

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets or sets the total number of items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Gets whether there is a previous page.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets whether there is a next page.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Creates a new paged result.</summary>
    public static PagedResult<T> Create(List<T> items, int page, int pageSize, int totalCount) =>
        new() { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
