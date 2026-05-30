using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Discount service implementation.</summary>
public class DiscountService : IDiscountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DiscountService> _logger;

    /// <summary>Initializes a new instance of DiscountService.</summary>
    public DiscountService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DiscountService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<DiscountDto>>> GetAllAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetAllAsync();
        return ApiResponse<List<DiscountDto>>.SuccessResponse(_mapper.Map<List<DiscountDto>>(discounts));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<DiscountDto>>> GetActiveAsync()
    {
        var discounts = await _unitOfWork.Discounts.GetActiveDiscountsAsync();
        return ApiResponse<List<DiscountDto>>.SuccessResponse(_mapper.Map<List<DiscountDto>>(discounts));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<DiscountDto>> GetByIdAsync(Guid id)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(id);
        if (discount == null)
            return ApiResponse<DiscountDto>.FailResponse("Discount not found");

        return ApiResponse<DiscountDto>.SuccessResponse(_mapper.Map<DiscountDto>(discount));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<DiscountDto>> CreateAsync(CreateDiscountRequest request, string userId)
    {
        if (await _unitOfWork.Discounts.GetByCodeAsync(request.Code) != null)
            return ApiResponse<DiscountDto>.FailResponse("A discount with this code already exists");

        var discount = _mapper.Map<Domain.Entities.Discount>(request);
        discount.CreatedBy = userId;
        discount.IsActive = true;

        await _unitOfWork.Discounts.AddAsync(discount);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Discount {DiscountId} created by {UserId}", discount.Id, userId);
        return ApiResponse<DiscountDto>.SuccessResponse(_mapper.Map<DiscountDto>(discount), "Discount created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<DiscountDto>> UpdateAsync(Guid id, UpdateDiscountRequest request, string userId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(id);
        if (discount == null)
            return ApiResponse<DiscountDto>.FailResponse("Discount not found");

        _mapper.Map(request, discount);
        discount.UpdatedBy = userId;

        _unitOfWork.Discounts.Update(discount);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Discount {DiscountId} updated by {UserId}", id, userId);
        return ApiResponse<DiscountDto>.SuccessResponse(_mapper.Map<DiscountDto>(discount), "Discount updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var discount = await _unitOfWork.Discounts.GetByIdAsync(id);
        if (discount == null)
            return ApiResponse<bool>.FailResponse("Discount not found");

        discount.UpdatedBy = userId;
        _unitOfWork.Discounts.SoftDelete(discount);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Discount {DiscountId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Discount deleted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<DiscountValidationResult>> ValidateAsync(ValidateDiscountRequest request)
    {
        var discount = await _unitOfWork.Discounts.GetByCodeAsync(request.Code);
        if (discount == null)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = "Invalid discount code"
            });

        if (!discount.IsActive)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = "Discount code is inactive"
            });

        var now = DateTime.UtcNow;
        if (now < discount.StartDate)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = "Discount code is not active yet"
            });

        if (now > discount.EndDate)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = "Discount code has expired"
            });

        if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit.Value)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = "Discount code has reached its maximum number of redemptions"
            });

        if (discount.MinOrderAmount.HasValue && request.OrderAmount < discount.MinOrderAmount.Value)
            return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
            {
                IsValid = false, Message = $"Minimum order amount not met. Required: {discount.MinOrderAmount.Value:C}"
            });

        decimal discountAmount = discount.DiscountType == DiscountType.Percentage
            ? request.OrderAmount * (discount.Value / 100)
            : discount.Value;

        if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
            discountAmount = discount.MaxDiscountAmount.Value;

        return ApiResponse<DiscountValidationResult>.SuccessResponse(new DiscountValidationResult
        {
            IsValid = true,
            DiscountAmount = discountAmount,
            Message = "Discount is valid"
        });
    }
}
