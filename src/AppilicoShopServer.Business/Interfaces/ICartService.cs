using AppilicoShopServer.Business.DTOs.Cart;
using AppilicoShopServer.Business.DTOs.Common;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Cart service interface.</summary>
public interface ICartService
{
    /// <summary>Gets the active cart for a customer.</summary>
    Task<ApiResponse<CartDto>> GetCartAsync(Guid customerId);

    /// <summary>Adds an item to the cart.</summary>
    Task<ApiResponse<CartDto>> AddItemAsync(Guid customerId, AddToCartRequest request);

    /// <summary>Updates a cart item quantity.</summary>
    Task<ApiResponse<CartDto>> UpdateItemAsync(Guid customerId, Guid cartItemId, UpdateCartItemRequest request);

    /// <summary>Removes an item from the cart.</summary>
    Task<ApiResponse<CartDto>> RemoveItemAsync(Guid customerId, Guid cartItemId);

    /// <summary>Clears the cart.</summary>
    Task<ApiResponse<bool>> ClearCartAsync(Guid customerId);
}
