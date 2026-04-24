using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Wishlist;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Wishlist service interface.</summary>
public interface IWishlistService
{
    /// <summary>Gets wishlist for a customer.</summary>
    Task<ApiResponse<List<WishlistDto>>> GetByCustomerAsync(Guid customerId);

    /// <summary>Adds a product to wishlist.</summary>
    Task<ApiResponse<WishlistDto>> AddAsync(Guid customerId, Guid productId);

    /// <summary>Removes a product from wishlist.</summary>
    Task<ApiResponse<bool>> RemoveAsync(Guid customerId, Guid productId);

    /// <summary>Checks if a product is in wishlist.</summary>
    Task<ApiResponse<bool>> IsInWishlistAsync(Guid customerId, Guid productId);
}
