using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Discount;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Discount service interface.</summary>
public interface IDiscountService
{
    /// <summary>Gets all discounts.</summary>
    Task<ApiResponse<List<DiscountDto>>> GetAllAsync();

    /// <summary>Gets active discounts.</summary>
    Task<ApiResponse<List<DiscountDto>>> GetActiveAsync();

    /// <summary>Gets a discount by ID.</summary>
    Task<ApiResponse<DiscountDto>> GetByIdAsync(Guid id);

    /// <summary>Creates a discount.</summary>
    Task<ApiResponse<DiscountDto>> CreateAsync(CreateDiscountRequest request, string userId);

    /// <summary>Updates a discount.</summary>
    Task<ApiResponse<DiscountDto>> UpdateAsync(Guid id, UpdateDiscountRequest request, string userId);

    /// <summary>Deletes a discount.</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>Validates a discount code.</summary>
    Task<ApiResponse<DiscountValidationResult>> ValidateAsync(ValidateDiscountRequest request);
}
