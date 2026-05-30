using System.Text.Json;
using AppilicoShopServer.Business.Exceptions;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Visual;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AppilicoShopServer.Business.Services;

/// <summary>Visuals service implementation.</summary>
public class VisualService : IVisualService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFileStorageService _storage;
    private readonly ILogger<VisualService> _logger;

    public VisualService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, IFileStorageService storage, ILogger<VisualService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _storage = storage;
        _logger = logger;
    }

    private static VisualDto MapList(Visual v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Slug = v.Slug,
        Description = v.Description,
        Category = v.Category.ToString(),
        Type = v.Type,
        ThumbnailUrl = v.ThumbnailUrl ?? v.ImageUrl,
        ImageUrl = v.ImageUrl,
        RequiredPlan = v.RequiredPlan.ToString(),
        DownloadCount = v.DownloadCount,
        Tags = TryParseJson(v.Tags),
        DemoUrl = v.DemoUrl,
        SortOrder = v.SortOrder
    };

    private static VisualDetailDto MapDetail(Visual v) => new()
    {
        Id = v.Id,
        Name = v.Name,
        Slug = v.Slug,
        Description = v.Description,
        FullDescription = v.FullDescription,
        Category = v.Category.ToString(),
        Type = v.Type,
        ThumbnailUrl = v.ThumbnailUrl ?? v.ImageUrl,
        ImageUrl = v.ImageUrl,
        PreviewImageUrls = TryParseJson(v.PreviewImageUrls),
        RequiredPlan = v.RequiredPlan.ToString(),
        DownloadCount = v.DownloadCount,
        Tags = TryParseJson(v.Tags),
        TechnicalSpecs = v.TechnicalSpecs,
        DataRequirements = v.DataRequirements,
        PowerBIVersion = v.PowerBIVersion,
        DemoUrl = v.DemoUrl,
        SortOrder = v.SortOrder,
        CreatedAt = v.CreatedAt,
        UpdatedAt = v.UpdatedAt
    };

    private static List<string> TryParseJson(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<VisualDto>>> GetAllAsync()
    {
        var visuals = await _unitOfWork.Visuals.GetActiveOrderedAsync();
        return ApiResponse<List<VisualDto>>.SuccessResponse(visuals.Select(MapList).ToList());
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<VisualDto>>> GetPagedAsync(VisualFilterQuery query)
    {
        VisualCategory? category = null;
        if (!string.IsNullOrWhiteSpace(query.Category) && Enum.TryParse<VisualCategory>(query.Category, true, out var categoryValue))
            category = categoryValue;

        SubscriptionTier? requiredPlan = null;
        if (!string.IsNullOrWhiteSpace(query.RequiredPlan) && Enum.TryParse<SubscriptionTier>(query.RequiredPlan, true, out var planValue))
            requiredPlan = planValue;

        var normalized = PaginationRequest.Normalize(query.Page, query.PageSize);
        var (items, total) = await _unitOfWork.Visuals.GetPagedActiveAsync(
            category,
            requiredPlan,
            query.Search,
            normalized.Page,
            normalized.PageSize);

        return ApiResponse<PagedResult<VisualDto>>.SuccessResponse(
            PagedResult<VisualDto>.Create(items.Select(MapList).ToList(), normalized.Page, normalized.PageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> GetByIdAsync(Guid id)
    {
        var v = await _unitOfWork.Visuals.GetVisibleByIdAsync(id);
        if (v == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(v));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> GetBySlugAsync(string slug)
    {
        var v = await _unitOfWork.Visuals.GetVisibleBySlugAsync(slug);
        if (v == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(v));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDownloadResponseDto>> DownloadAsync(Guid id, string userId, string ipAddress)
    {
        var visual = await _unitOfWork.Visuals.GetVisibleByIdAsync(id, requireActive: true);
        if (visual == null)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse("Visual not found");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse("User not found");

        if (user.SubscriptionTier < visual.RequiredPlan)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse(
                $"Upgrade to {visual.RequiredPlan} required to download this visual");

        var fileUrl = visual.DemoUrl ?? $"/api/v1/visuals/{id}/file";
        string presignedUrl;
        try
        {
            presignedUrl = await _storage.GetPresignedUrlAsync(fileUrl, 30);
        }
        catch (Exception ex) when (ex is StorageProviderException or NotSupportedException)
        {
            _logger.LogWarning(ex, "Visual download rejected because storage is unavailable for visual {VisualId}", id);
            return ApiResponse<VisualDownloadResponseDto>.FailResponse("Visual downloads are temporarily unavailable.");
        }

        await _unitOfWork.Visuals.AddDownloadAsync(new VisualDownload
        {
            VisualId = visual.Id,
            UserId = userId,
            DownloadedAt = DateTime.UtcNow,
            IPAddress = ipAddress
        });

        visual.DownloadCount++;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<VisualDownloadResponseDto>.SuccessResponse(new VisualDownloadResponseDto
        {
            DownloadUrl = presignedUrl,
            FileName = $"{visual.Slug}.pbiviz",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        });
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> CreateAsync(UpsertVisualRequest request)
    {
        if (await _unitOfWork.Visuals.SlugExistsAsync(request.Slug))
            return ApiResponse<VisualDetailDto>.FailResponse("A visual with this slug already exists");

        Enum.TryParse<VisualCategory>(request.Category, true, out var cat);
        Enum.TryParse<SubscriptionTier>(request.RequiredPlan, true, out var tier);

        var visual = new Visual
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            FullDescription = request.FullDescription,
            Category = cat,
            RequiredPlan = tier,
            ThumbnailUrl = request.ThumbnailUrl,
            PreviewImageUrls = JsonSerializer.Serialize(request.PreviewImageUrls),
            Tags = JsonSerializer.Serialize(request.Tags),
            TechnicalSpecs = request.TechnicalSpecs,
            DataRequirements = request.DataRequirements,
            PowerBIVersion = request.PowerBIVersion,
            DemoUrl = request.DemoUrl,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.Visuals.AddAsync(visual);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(visual));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> UpdateAsync(Guid id, UpsertVisualRequest request)
    {
        var visual = await _unitOfWork.Visuals.GetVisibleByIdAsync(id);
        if (visual == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");

        if (await _unitOfWork.Visuals.SlugExistsAsync(request.Slug, id))
            return ApiResponse<VisualDetailDto>.FailResponse("A visual with this slug already exists");

        Enum.TryParse<VisualCategory>(request.Category, true, out var cat);
        Enum.TryParse<SubscriptionTier>(request.RequiredPlan, true, out var tier);

        visual.Name = request.Name;
        visual.Slug = request.Slug;
        visual.Description = request.Description;
        visual.FullDescription = request.FullDescription;
        visual.Category = cat;
        visual.RequiredPlan = tier;
        visual.ThumbnailUrl = request.ThumbnailUrl;
        visual.PreviewImageUrls = JsonSerializer.Serialize(request.PreviewImageUrls);
        visual.Tags = JsonSerializer.Serialize(request.Tags);
        visual.TechnicalSpecs = request.TechnicalSpecs;
        visual.DataRequirements = request.DataRequirements;
        visual.PowerBIVersion = request.PowerBIVersion;
        visual.DemoUrl = request.DemoUrl;
        visual.SortOrder = request.SortOrder;
        visual.IsActive = request.IsActive;

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(visual));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var visual = await _unitOfWork.Visuals.GetVisibleByIdAsync(id);
        if (visual == null) return ApiResponse<bool>.FailResponse("Visual not found");

        visual.IsDeleted = true;
        visual.IsActive = false;
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
