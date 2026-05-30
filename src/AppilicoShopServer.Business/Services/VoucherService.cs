using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Voucher;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Voucher service implementation.</summary>
public class VoucherService : IVoucherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VoucherService> _logger;

    /// <summary>Initializes a new instance of VoucherService.</summary>
    public VoucherService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VoucherService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<VoucherDto>>> GetAllAsync()
    {
        var vouchers = await _unitOfWork.Vouchers.GetAllAsync();
        return ApiResponse<List<VoucherDto>>.SuccessResponse(_mapper.Map<List<VoucherDto>>(vouchers));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VoucherDto>> GetByIdAsync(Guid id)
    {
        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
        if (voucher == null)
            return ApiResponse<VoucherDto>.FailResponse("Voucher not found");

        return ApiResponse<VoucherDto>.SuccessResponse(_mapper.Map<VoucherDto>(voucher));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VoucherDto>> CreateAsync(CreateVoucherRequest request, string userId)
    {
        if (await _unitOfWork.Vouchers.GetByCodeAsync(request.Code) != null)
            return ApiResponse<VoucherDto>.FailResponse("A voucher with this code already exists");

        var voucher = _mapper.Map<Domain.Entities.Voucher>(request);
        voucher.CreatedBy = userId;
        voucher.IsActive = true;

        await _unitOfWork.Vouchers.AddAsync(voucher);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Voucher {VoucherId} created by {UserId}", voucher.Id, userId);
        return ApiResponse<VoucherDto>.SuccessResponse(_mapper.Map<VoucherDto>(voucher), "Voucher created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VoucherDto>> UpdateAsync(Guid id, UpdateVoucherRequest request, string userId)
    {
        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
        if (voucher == null)
            return ApiResponse<VoucherDto>.FailResponse("Voucher not found");

        _mapper.Map(request, voucher);
        voucher.UpdatedBy = userId;

        _unitOfWork.Vouchers.Update(voucher);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Voucher {VoucherId} updated by {UserId}", id, userId);
        return ApiResponse<VoucherDto>.SuccessResponse(_mapper.Map<VoucherDto>(voucher), "Voucher updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id);
        if (voucher == null)
            return ApiResponse<bool>.FailResponse("Voucher not found");

        voucher.UpdatedBy = userId;
        _unitOfWork.Vouchers.SoftDelete(voucher);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Voucher {VoucherId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Voucher deleted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VoucherValidationResult>> ValidateAsync(ValidateVoucherRequest request)
    {
        var voucher = await _unitOfWork.Vouchers.GetByCodeAsync(request.Code);
        if (voucher == null || !voucher.IsActive)
            return ApiResponse<VoucherValidationResult>.SuccessResponse(new VoucherValidationResult
            {
                IsValid = false, Message = "Invalid voucher code"
            });

        var now = DateTime.UtcNow;
        if (now < voucher.StartDate || now > voucher.ExpiryDate)
            return ApiResponse<VoucherValidationResult>.SuccessResponse(new VoucherValidationResult
            {
                IsValid = false, Message = "Voucher is not currently active"
            });

        if (voucher.MaxRedemptions.HasValue && voucher.CurrentRedemptions >= voucher.MaxRedemptions.Value)
            return ApiResponse<VoucherValidationResult>.SuccessResponse(new VoucherValidationResult
            {
                IsValid = false, Message = "Voucher redemption limit reached"
            });

        if (voucher.MinOrderAmount.HasValue && request.OrderAmount < voucher.MinOrderAmount.Value)
            return ApiResponse<VoucherValidationResult>.SuccessResponse(new VoucherValidationResult
            {
                IsValid = false, Message = $"Minimum order amount is {voucher.MinOrderAmount.Value:C}"
            });

        decimal discountAmount = voucher.ValueType == VoucherValueType.Percentage
            ? request.OrderAmount * (voucher.Value / 100)
            : voucher.Value;

        return ApiResponse<VoucherValidationResult>.SuccessResponse(new VoucherValidationResult
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            Message = "Voucher is valid"
        });
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> RedeemAsync(RedeemVoucherRequest request, Guid customerId)
    {
        var voucher = await _unitOfWork.Vouchers.GetByCodeAsync(request.Code);
        if (voucher == null || !voucher.IsActive)
            return ApiResponse<bool>.FailResponse("Invalid voucher");

        voucher.CurrentRedemptions++;
        if (voucher.IsSingleUse)
            voucher.IsActive = false;

        var redemption = new VoucherRedemption
        {
            VoucherId = voucher.Id,
            CustomerId = customerId,
            OrderId = request.OrderId,
            RedeemedAt = DateTime.UtcNow
        };

        _unitOfWork.Vouchers.Update(voucher);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Voucher {Code} redeemed by customer {CustomerId}", request.Code, customerId);
        return ApiResponse<bool>.SuccessResponse(true, "Voucher redeemed successfully");
    }
}
