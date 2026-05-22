using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Customer;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Customer service interface.</summary>
public interface ICustomerService
{
    /// <summary>Gets all customers with pagination.</summary>
    Task<ApiResponse<List<CustomerDto>>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);

    /// <summary>Gets a customer by ID.</summary>
    Task<ApiResponse<CustomerDto>> GetByIdAsync(Guid id);

    /// <summary>Gets a customer by user ID.</summary>
    Task<ApiResponse<CustomerDto>> GetByUserIdAsync(string userId);

    /// <summary>Updates a customer.</summary>
    Task<ApiResponse<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, string userId, bool canManageMembershipTier = false);

    /// <summary>Gets customer loyalty info.</summary>
    Task<ApiResponse<CustomerLoyaltyDto>> GetLoyaltyAsync(Guid customerId);

    /// <summary>Adds loyalty points.</summary>
    Task<ApiResponse<CustomerLoyaltyDto>> AddLoyaltyPointsAsync(Guid customerId, int points);

    /// <summary>Gets addresses for a customer by user ID.</summary>
    Task<ApiResponse<List<CustomerAddressDto>>> GetAddressesAsync(string userId);

    /// <summary>Creates a new address for a customer.</summary>
    Task<ApiResponse<CustomerAddressDto>> CreateAddressAsync(string userId, CreateAddressRequest request);

    /// <summary>Updates an existing address.</summary>
    Task<ApiResponse<CustomerAddressDto>> UpdateAddressAsync(string userId, Guid addressId, UpdateAddressRequest request);

    /// <summary>Deletes an address.</summary>
    Task<ApiResponse<bool>> DeleteAddressAsync(string userId, Guid addressId);
}
