using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Offer;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Special offer service implementation.</summary>
public class SpecialOfferService : ISpecialOfferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SpecialOfferService> _logger;

    /// <summary>Initializes a new instance of SpecialOfferService.</summary>
    public SpecialOfferService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SpecialOfferService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<SpecialOfferDto>>> GetAllAsync()
    {
        var offers = await _unitOfWork.SpecialOffers.GetAllAsync();
        return ApiResponse<List<SpecialOfferDto>>.SuccessResponse(_mapper.Map<List<SpecialOfferDto>>(offers));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<SpecialOfferDto>>> GetActiveAsync()
    {
        var offers = await _unitOfWork.SpecialOffers.GetActiveOffersAsync();
        return ApiResponse<List<SpecialOfferDto>>.SuccessResponse(_mapper.Map<List<SpecialOfferDto>>(offers));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SpecialOfferDto>> GetByIdAsync(Guid id)
    {
        var offer = await _unitOfWork.SpecialOffers.GetByIdAsync(id);
        if (offer == null)
            return ApiResponse<SpecialOfferDto>.FailResponse("Special offer not found");

        return ApiResponse<SpecialOfferDto>.SuccessResponse(_mapper.Map<SpecialOfferDto>(offer));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SpecialOfferDto>> CreateAsync(CreateSpecialOfferRequest request, string userId)
    {
        var offer = _mapper.Map<SpecialOffer>(request);
        offer.CreatedBy = userId;
        offer.IsActive = true;

        await _unitOfWork.SpecialOffers.AddAsync(offer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Special offer {OfferId} created by {UserId}", offer.Id, userId);
        return ApiResponse<SpecialOfferDto>.SuccessResponse(_mapper.Map<SpecialOfferDto>(offer), "Special offer created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SpecialOfferDto>> UpdateAsync(Guid id, UpdateSpecialOfferRequest request, string userId)
    {
        var offer = await _unitOfWork.SpecialOffers.GetByIdAsync(id);
        if (offer == null)
            return ApiResponse<SpecialOfferDto>.FailResponse("Special offer not found");

        _mapper.Map(request, offer);
        offer.UpdatedBy = userId;

        _unitOfWork.SpecialOffers.Update(offer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Special offer {OfferId} updated by {UserId}", id, userId);
        return ApiResponse<SpecialOfferDto>.SuccessResponse(_mapper.Map<SpecialOfferDto>(offer), "Special offer updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var offer = await _unitOfWork.SpecialOffers.GetByIdAsync(id);
        if (offer == null)
            return ApiResponse<bool>.FailResponse("Special offer not found");

        offer.UpdatedBy = userId;
        _unitOfWork.SpecialOffers.SoftDelete(offer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Special offer {OfferId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Special offer deleted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SpecialOfferDto>> AddProductsAsync(Guid offerId, AddOfferProductsRequest request, string userId)
    {
        var offer = await _unitOfWork.SpecialOffers.GetWithProductsAsync(offerId);
        if (offer == null)
            return ApiResponse<SpecialOfferDto>.FailResponse("Special offer not found");

        foreach (var item in request.Products)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null) continue;

            var offerProduct = new SpecialOfferProduct
            {
                SpecialOfferId = offerId,
                ProductId = item.ProductId,
                OfferPrice = item.OfferPrice,
                MaxQuantityPerCustomer = item.MaxQuantityPerCustomer,
                CreatedBy = userId
            };

            offer.SpecialOfferProducts.Add(offerProduct);
        }

        _unitOfWork.SpecialOffers.Update(offer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Products added to offer {OfferId} by {UserId}", offerId, userId);
        return ApiResponse<SpecialOfferDto>.SuccessResponse(_mapper.Map<SpecialOfferDto>(offer), "Products added to offer");
    }
}
