using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Brand;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Brand service interface.</summary>
public interface IBrandService
{
    /// <summary>Gets all brands.</summary>
    Task<ApiResponse<List<BrandDto>>> GetAllAsync();

    /// <summary>Gets a brand by ID.</summary>
    Task<ApiResponse<BrandDto>> GetByIdAsync(Guid id);

    /// <summary>Creates a brand.</summary>
    Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequest request, string userId);

    /// <summary>Updates a brand.</summary>
    Task<ApiResponse<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request, string userId);

    /// <summary>Deletes a brand (soft).</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
}
