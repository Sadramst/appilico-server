using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for SpecialOffer-specific operations.
/// </summary>
public interface ISpecialOfferRepository : IGenericRepository<SpecialOffer>
{
    /// <summary>Gets active offers with products.</summary>
    Task<IReadOnlyList<SpecialOffer>> GetActiveOffersAsync();

    /// <summary>Gets an offer with its products.</summary>
    Task<SpecialOffer?> GetWithProductsAsync(Guid id);
}
