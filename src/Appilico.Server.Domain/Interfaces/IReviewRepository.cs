using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductReview-specific operations.
/// </summary>
public interface IReviewRepository : IGenericRepository<ProductReview>
{
    /// <summary>Gets reviews by product ID.</summary>
    Task<IReadOnlyList<ProductReview>> GetByProductIdAsync(Guid productId);

    /// <summary>Gets reviews by customer ID.</summary>
    Task<IReadOnlyList<ProductReview>> GetByCustomerIdAsync(Guid customerId);
}
