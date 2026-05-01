using Microsoft.EntityFrameworkCore;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Visual;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;

namespace Appilico.Server.Business.Services;

/// <summary>Visuals service implementation.</summary>
public class VisualService : IVisualService
{
    private readonly AppDbContext _db;

    public VisualService(AppDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<ApiResponse<List<VisualDto>>> GetAllAsync()
    {
        var visuals = await _db.Visuals
            .Where(v => v.IsActive && !v.IsDeleted)
            .OrderBy(v => v.SortOrder)
            .Select(v => new VisualDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = v.Description,
                Category = v.Category,
                Type = v.Type,
                ImageUrl = v.ImageUrl,
                DemoUrl = v.DemoUrl,
                SortOrder = v.SortOrder
            })
            .ToListAsync();

        return ApiResponse<List<VisualDto>>.SuccessResponse(visuals);
    }
}
