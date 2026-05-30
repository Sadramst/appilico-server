using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Offer;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Special offer service interface.</summary>
public interface ISpecialOfferService
{
    /// <summary>Gets all offers.</summary>
    Task<ApiResponse<List<SpecialOfferDto>>> GetAllAsync();

    /// <summary>Gets active offers.</summary>
    Task<ApiResponse<List<SpecialOfferDto>>> GetActiveAsync();

    /// <summary>Gets an offer by ID.</summary>
    Task<ApiResponse<SpecialOfferDto>> GetByIdAsync(Guid id);

    /// <summary>Creates an offer.</summary>
    Task<ApiResponse<SpecialOfferDto>> CreateAsync(CreateSpecialOfferRequest request, string userId);

    /// <summary>Updates an offer.</summary>
    Task<ApiResponse<SpecialOfferDto>> UpdateAsync(Guid id, UpdateSpecialOfferRequest request, string userId);

    /// <summary>Deletes an offer.</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>Adds products to an offer.</summary>
    Task<ApiResponse<SpecialOfferDto>> AddProductsAsync(Guid offerId, AddOfferProductsRequest request, string userId);
}
