using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Review;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Review service implementation.</summary>
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReviewService> _logger;

    /// <summary>Initializes a new instance of ReviewService.</summary>
    public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ReviewDto>>> GetByProductAsync(Guid productId, int page = 1, int pageSize = 10)
    {
        var (items, totalCount) = await _unitOfWork.Reviews.GetPagedAsync(page, pageSize,
            predicate: r => r.ProductId == productId && r.IsApproved,
            orderBy: q => q.OrderByDescending(r => r.CreatedAt),
            includes: r => r.Customer);

        var dtos = _mapper.Map<List<ReviewDto>>(items);
        var pagination = PaginationMeta.Create(page, pageSize, totalCount);

        return ApiResponse<List<ReviewDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReviewDto>> GetByIdAsync(Guid id)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
            return ApiResponse<ReviewDto>.FailResponse("Review not found");

        return ApiResponse<ReviewDto>.SuccessResponse(_mapper.Map<ReviewDto>(review));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReviewDto>> CreateAsync(Guid customerId, CreateReviewRequest request, string userId)
    {
        // Check if customer already reviewed this product
        if (await _unitOfWork.Reviews.AnyAsync(r => r.ProductId == request.ProductId && r.CustomerId == customerId))
            return ApiResponse<ReviewDto>.FailResponse("You have already reviewed this product");

        var review = _mapper.Map<ProductReview>(request);
        review.CustomerId = customerId;
        review.IsApproved = false;
        review.CreatedBy = userId;

        // Check if verified purchase
        review.IsVerifiedPurchase = await _unitOfWork.Orders.AnyAsync(o =>
            o.CustomerId == customerId && o.Items.Any(i => i.ProductId == request.ProductId));

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Review created for product {ProductId} by customer {CustomerId}", request.ProductId, customerId);
        return ApiResponse<ReviewDto>.SuccessResponse(_mapper.Map<ReviewDto>(review), "Review submitted for approval");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ReviewDto>> UpdateAsync(Guid id, UpdateReviewRequest request, string userId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
            return ApiResponse<ReviewDto>.FailResponse("Review not found");

        _mapper.Map(request, review);
        review.UpdatedBy = userId;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ReviewDto>.SuccessResponse(_mapper.Map<ReviewDto>(review), "Review updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
            return ApiResponse<bool>.FailResponse("Review not found");

        review.UpdatedBy = userId;
        _unitOfWork.Reviews.SoftDelete(review);
        await _unitOfWork.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        return ApiResponse<bool>.SuccessResponse(true, "Review deleted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> ApproveAsync(Guid id, string userId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(id);
        if (review == null)
            return ApiResponse<bool>.FailResponse("Review not found");

        review.IsApproved = true;
        review.UpdatedBy = userId;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        _logger.LogInformation("Review {ReviewId} approved by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Review approved");
    }

    private async Task UpdateProductRatingAsync(Guid productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null) return;

        var reviews = await _unitOfWork.Reviews.FindAsync(r => r.ProductId == productId && r.IsApproved);
        if (reviews.Any())
        {
            product.AverageRating = (decimal)reviews.Average(r => r.Rating);
            product.TotalReviews = reviews.Count;
        }
        else
        {
            product.AverageRating = 0;
            product.TotalReviews = 0;
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
    }
}
