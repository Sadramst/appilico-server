using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Brand;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Brand service implementation.</summary>
public class BrandService : IBrandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BrandService> _logger;

    /// <summary>Initializes a new instance of BrandService.</summary>
    public BrandService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BrandService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<BrandDto>>> GetAllAsync()
    {
        var brands = await _unitOfWork.Brands.GetAllAsync();
        return ApiResponse<List<BrandDto>>.SuccessResponse(_mapper.Map<List<BrandDto>>(brands));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BrandDto>> GetByIdAsync(Guid id)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            return ApiResponse<BrandDto>.FailResponse("Brand not found");

        return ApiResponse<BrandDto>.SuccessResponse(_mapper.Map<BrandDto>(brand));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BrandDto>> CreateAsync(CreateBrandRequest request, string userId)
    {
        var brand = _mapper.Map<Domain.Entities.Brand>(request);
        brand.CreatedBy = userId;

        await _unitOfWork.Brands.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} created by {UserId}", brand.Id, userId);
        return ApiResponse<BrandDto>.SuccessResponse(_mapper.Map<BrandDto>(brand), "Brand created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BrandDto>> UpdateAsync(Guid id, UpdateBrandRequest request, string userId)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            return ApiResponse<BrandDto>.FailResponse("Brand not found");

        _mapper.Map(request, brand);
        brand.UpdatedBy = userId;

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} updated by {UserId}", id, userId);
        return ApiResponse<BrandDto>.SuccessResponse(_mapper.Map<BrandDto>(brand), "Brand updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(id);
        if (brand == null)
            return ApiResponse<bool>.FailResponse("Brand not found");

        brand.UpdatedBy = userId;
        _unitOfWork.Brands.SoftDelete(brand);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Brand {BrandId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Brand deleted successfully");
    }
}
