using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Category;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Category service interface.</summary>
public interface ICategoryService
{
    /// <summary>Gets all categories.</summary>
    Task<ApiResponse<List<CategoryDto>>> GetAllAsync();

    /// <summary>Gets category tree.</summary>
    Task<ApiResponse<List<CategoryDto>>> GetCategoryTreeAsync();

    /// <summary>Gets a category by ID.</summary>
    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id);

    /// <summary>Creates a category.</summary>
    Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, string userId);

    /// <summary>Updates a category.</summary>
    Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, string userId);

    /// <summary>Deletes a category (soft).</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
}
