using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository/query abstraction for waitlist entries.</summary>
public interface IWaitlistRepository : IGenericRepository<WaitlistEntry>
{
    /// <summary>Gets a non-deleted entry by normalized email.</summary>
    Task<WaitlistEntry?> GetByEmailAsync(string email);

    /// <summary>Counts non-deleted entries.</summary>
    Task<int> CountActiveAsync();

    /// <summary>Gets a filtered page for admin views.</summary>
    Task<(IReadOnlyList<WaitlistEntry> Items, int TotalCount)> GetAdminPageAsync(int page, int pageSize, bool? isNotified);

    /// <summary>Gets a non-deleted entry by ID.</summary>
    Task<WaitlistEntry?> GetActiveByIdAsync(Guid id);
}