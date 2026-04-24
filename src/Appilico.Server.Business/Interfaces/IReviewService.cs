using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Review;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Review service interface.</summary>
public interface IReviewService
{
    /// <summary>Gets reviews for a product.</summary>
    Task<ApiResponse<List<ReviewDto>>> GetByProductAsync(Guid productId, int page = 1, int pageSize = 10);

    /// <summary>Gets a review by ID.</summary>
    Task<ApiResponse<ReviewDto>> GetByIdAsync(Guid id);

    /// <summary>Creates a review.</summary>
    Task<ApiResponse<ReviewDto>> CreateAsync(Guid customerId, CreateReviewRequest request, string userId);

    /// <summary>Updates a review.</summary>
    Task<ApiResponse<ReviewDto>> UpdateAsync(Guid id, UpdateReviewRequest request, string userId);

    /// <summary>Deletes a review.</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>Approves a review.</summary>
    Task<ApiResponse<bool>> ApproveAsync(Guid id, string userId);
}
