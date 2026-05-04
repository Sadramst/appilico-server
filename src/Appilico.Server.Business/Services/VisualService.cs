using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Visual;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.Services;

/// <summary>Visuals service implementation.</summary>
public class VisualService : IVisualService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storage;

    public VisualService(AppDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
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
        var visuals = await _db.Visuals
            .Where(v => v.IsActive && !v.IsDeleted)
            .OrderBy(v => v.SortOrder)
            .ToListAsync();
        return ApiResponse<List<VisualDto>>.SuccessResponse(visuals.Select(MapList).ToList());
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<VisualDto>>> GetPagedAsync(VisualFilterQuery query)
    {
        var q = _db.Visuals.Where(v => v.IsActive && !v.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            Enum.TryParse<VisualCategory>(query.Category, true, out var catEnum))
            q = q.Where(v => v.Category == catEnum);

        if (!string.IsNullOrWhiteSpace(query.RequiredPlan) &&
            Enum.TryParse<SubscriptionTier>(query.RequiredPlan, true, out var planEnum))
            q = q.Where(v => v.RequiredPlan == planEnum);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(v => v.Name.Contains(query.Search) || v.Description.Contains(query.Search));

        var total = await q.CountAsync();
        var items = await q.OrderBy(v => v.SortOrder)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<VisualDto>>.SuccessResponse(
            PagedResult<VisualDto>.Create(items.Select(MapList).ToList(), query.Page, query.PageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> GetByIdAsync(Guid id)
    {
        var v = await _db.Visuals.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (v == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(v));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> GetBySlugAsync(string slug)
    {
        var v = await _db.Visuals.FirstOrDefaultAsync(x => x.Slug == slug && !x.IsDeleted);
        if (v == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(v));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDownloadResponseDto>> DownloadAsync(Guid id, string userId, string ipAddress)
    {
        var visual = await _db.Visuals.FirstOrDefaultAsync(v => v.Id == id && v.IsActive && !v.IsDeleted);
        if (visual == null)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse("Visual not found");

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse("User not found");

        // Check subscription tier
        var appUser = user as Domain.Entities.AppUser;
        if (appUser != null && appUser.SubscriptionTier < visual.RequiredPlan)
            return ApiResponse<VisualDownloadResponseDto>.FailResponse(
                $"Upgrade to {visual.RequiredPlan} required to download this visual");

        // Log download
        _db.VisualDownloads.Add(new VisualDownload
        {
            VisualId = visual.Id,
            UserId = userId,
            DownloadedAt = DateTime.UtcNow,
            IPAddress = ipAddress
        });

        visual.DownloadCount++;
        await _db.SaveChangesAsync();

        // Generate pre-signed URL (30 min) or stub
        var fileUrl = visual.DemoUrl ?? $"/api/v1/visuals/{id}/file";
        var presignedUrl = await _storage.GetPresignedUrlAsync(fileUrl, 30);

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
        if (await _db.Visuals.AnyAsync(v => v.Slug == request.Slug))
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

        _db.Visuals.Add(visual);
        await _db.SaveChangesAsync();
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(visual));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<VisualDetailDto>> UpdateAsync(Guid id, UpsertVisualRequest request)
    {
        var visual = await _db.Visuals.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (visual == null) return ApiResponse<VisualDetailDto>.FailResponse("Visual not found");

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

        await _db.SaveChangesAsync();
        return ApiResponse<VisualDetailDto>.SuccessResponse(MapDetail(visual));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        var visual = await _db.Visuals.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        if (visual == null) return ApiResponse<bool>.FailResponse("Visual not found");

        visual.IsDeleted = true;
        visual.IsActive = false;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
