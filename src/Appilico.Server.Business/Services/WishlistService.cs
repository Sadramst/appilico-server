using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Wishlist;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Wishlist service implementation.</summary>
public class WishlistService : IWishlistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<WishlistService> _logger;

    /// <summary>Initializes a new instance of WishlistService.</summary>
    public WishlistService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<WishlistService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<WishlistDto>>> GetByCustomerAsync(Guid customerId)
    {
        var wishlists = await _unitOfWork.Wishlists.FindAsync(w => w.CustomerId == customerId);
        return ApiResponse<List<WishlistDto>>.SuccessResponse(_mapper.Map<List<WishlistDto>>(wishlists));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WishlistDto>> AddAsync(Guid customerId, Guid productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return ApiResponse<WishlistDto>.FailResponse("Product not found");

        if (await _unitOfWork.Wishlists.AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId))
            return ApiResponse<WishlistDto>.FailResponse("Product is already in your wishlist");

        var wishlist = new Domain.Entities.Wishlist
        {
            CustomerId = customerId,
            ProductId = productId,
            AddedAt = DateTime.UtcNow
        };

        await _unitOfWork.Wishlists.AddAsync(wishlist);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<WishlistDto>.SuccessResponse(_mapper.Map<WishlistDto>(wishlist), "Added to wishlist");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> RemoveAsync(Guid customerId, Guid productId)
    {
        var wishlist = await _unitOfWork.Wishlists.FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId);
        if (wishlist == null)
            return ApiResponse<bool>.FailResponse("Item not found in wishlist");

        _unitOfWork.Wishlists.SoftDelete(wishlist);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Removed from wishlist");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> IsInWishlistAsync(Guid customerId, Guid productId)
    {
        var exists = await _unitOfWork.Wishlists.AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId);
        return ApiResponse<bool>.SuccessResponse(exists);
    }
}
