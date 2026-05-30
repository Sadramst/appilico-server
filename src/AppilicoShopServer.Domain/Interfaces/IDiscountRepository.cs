using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Discount-specific operations.
/// </summary>
public interface IDiscountRepository : IGenericRepository<Discount>
{
    /// <summary>Gets a discount by code.</summary>
    Task<Discount?> GetByCodeAsync(string code);

    /// <summary>Gets active discounts.</summary>
    Task<IReadOnlyList<Discount>> GetActiveDiscountsAsync();
}
