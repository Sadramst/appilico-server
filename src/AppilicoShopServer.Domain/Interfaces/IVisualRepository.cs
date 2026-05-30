using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository/query abstraction for visuals and visual downloads.</summary>
public interface IVisualRepository : IGenericRepository<Visual>
{
    /// <summary>Gets all active visuals ordered for display.</summary>
    Task<IReadOnlyList<Visual>> GetActiveOrderedAsync();

    /// <summary>Gets a filtered page of active visuals.</summary>
    Task<(IReadOnlyList<Visual> Items, int TotalCount)> GetPagedActiveAsync(
        VisualCategory? category,
        SubscriptionTier? requiredPlan,
        string? search,
        int page,
        int pageSize);

    /// <summary>Gets a non-deleted visual by ID.</summary>
    Task<Visual?> GetVisibleByIdAsync(Guid id, bool requireActive = false);

    /// <summary>Gets a non-deleted visual by slug.</summary>
    Task<Visual?> GetVisibleBySlugAsync(string slug);

    /// <summary>Checks whether a visual slug already exists.</summary>
    Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null);

    /// <summary>Adds a visual download audit record.</summary>
    Task AddDownloadAsync(VisualDownload download);
}