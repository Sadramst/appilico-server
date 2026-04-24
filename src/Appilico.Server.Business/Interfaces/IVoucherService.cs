using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Voucher;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Voucher service interface.</summary>
public interface IVoucherService
{
    /// <summary>Gets all vouchers.</summary>
    Task<ApiResponse<List<VoucherDto>>> GetAllAsync();

    /// <summary>Gets a voucher by ID.</summary>
    Task<ApiResponse<VoucherDto>> GetByIdAsync(Guid id);

    /// <summary>Creates a voucher.</summary>
    Task<ApiResponse<VoucherDto>> CreateAsync(CreateVoucherRequest request, string userId);

    /// <summary>Updates a voucher.</summary>
    Task<ApiResponse<VoucherDto>> UpdateAsync(Guid id, UpdateVoucherRequest request, string userId);

    /// <summary>Deletes a voucher.</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>Validates a voucher code.</summary>
    Task<ApiResponse<VoucherValidationResult>> ValidateAsync(ValidateVoucherRequest request);

    /// <summary>Redeems a voucher.</summary>
    Task<ApiResponse<bool>> RedeemAsync(RedeemVoucherRequest request, Guid customerId);
}
