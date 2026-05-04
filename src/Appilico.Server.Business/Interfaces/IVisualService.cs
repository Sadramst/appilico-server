using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Visual;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Visuals service interface.</summary>
public interface IVisualService
{
    /// <summary>Gets all active visuals (legacy — no filter).</summary>
    Task<ApiResponse<List<VisualDto>>> GetAllAsync();

    /// <summary>Gets paginated visuals with optional filters.</summary>
    Task<ApiResponse<PagedResult<VisualDto>>> GetPagedAsync(VisualFilterQuery query);

    /// <summary>Gets a visual by ID.</summary>
    Task<ApiResponse<VisualDetailDto>> GetByIdAsync(Guid id);

    /// <summary>Gets a visual by slug.</summary>
    Task<ApiResponse<VisualDetailDto>> GetBySlugAsync(string slug);

    /// <summary>Downloads a visual (checks subscription tier).</summary>
    Task<ApiResponse<VisualDownloadResponseDto>> DownloadAsync(Guid id, string userId, string ipAddress);

    /// <summary>Creates a visual (Admin).</summary>
    Task<ApiResponse<VisualDetailDto>> CreateAsync(UpsertVisualRequest request);

    /// <summary>Updates a visual (Admin).</summary>
    Task<ApiResponse<VisualDetailDto>> UpdateAsync(Guid id, UpsertVisualRequest request);

    /// <summary>Soft-deletes a visual (Admin).</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id);
}
