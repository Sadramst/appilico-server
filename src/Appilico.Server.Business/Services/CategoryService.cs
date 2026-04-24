using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Category;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Category service implementation.</summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    /// <summary>Initializes a new instance of CategoryService.</summary>
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CategoryDto>>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return ApiResponse<List<CategoryDto>>.SuccessResponse(_mapper.Map<List<CategoryDto>>(categories));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<CategoryDto>>> GetCategoryTreeAsync()
    {
        var tree = await _unitOfWork.Categories.GetCategoryTreeAsync();
        return ApiResponse<List<CategoryDto>>.SuccessResponse(_mapper.Map<List<CategoryDto>>(tree));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<CategoryDto>.FailResponse("Category not found");

        return ApiResponse<CategoryDto>.SuccessResponse(_mapper.Map<CategoryDto>(category));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, string userId)
    {
        var category = _mapper.Map<Domain.Entities.Category>(request);
        category.CreatedBy = userId;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} created by {UserId}", category.Id, userId);
        return ApiResponse<CategoryDto>.SuccessResponse(_mapper.Map<CategoryDto>(category), "Category created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, string userId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<CategoryDto>.FailResponse("Category not found");

        _mapper.Map(request, category);
        category.UpdatedBy = userId;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} updated by {UserId}", id, userId);
        return ApiResponse<CategoryDto>.SuccessResponse(_mapper.Map<CategoryDto>(category), "Category updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<bool>.FailResponse("Category not found");

        category.UpdatedBy = userId;
        _unitOfWork.Categories.SoftDelete(category);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Category {CategoryId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
    }
}
