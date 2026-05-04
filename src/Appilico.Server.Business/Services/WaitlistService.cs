using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Waitlist;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Services;

/// <summary>Waitlist service implementation.</summary>
public class WaitlistService : IWaitlistService
{
    private readonly AppDbContext _db;
    private readonly ILogger<WaitlistService> _logger;
    private readonly IEmailService _emailService;

    public WaitlistService(AppDbContext db, ILogger<WaitlistService> logger, IEmailService emailService)
    {
        _db = db;
        _logger = logger;
        _emailService = emailService;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<WaitlistSubscribeResponse>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _db.WaitlistEntries.FirstOrDefaultAsync(w => w.Email == email && !w.IsDeleted);
        var totalCount = await _db.WaitlistEntries.CountAsync(w => !w.IsDeleted);

        if (existing != null)
        {
            return ApiResponse<WaitlistSubscribeResponse>.SuccessResponse(new WaitlistSubscribeResponse
            {
                Position = existing.Position,
                TotalCount = totalCount,
                Message = $"You are already on the waitlist at position #{existing.Position}."
            }, "Already subscribed");
        }

        var position = totalCount + 1;
        var entry = new WaitlistEntry
        {
            Email = email,
            Company = request.Company,
            Role = request.Role,
            Plan = request.Plan ?? request.InterestedPlan,
            InterestedPlan = request.InterestedPlan ?? request.Plan,
            Position = position,
            IPAddress = ipAddress
        };

        _db.WaitlistEntries.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Waitlist entry added: {Email} at position {Position}", email, position);

        try
        {
            await _emailService.SendWaitlistConfirmationAsync(email, position);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send waitlist confirmation to {Email}", email);
        }

        return ApiResponse<WaitlistSubscribeResponse>.SuccessResponse(new WaitlistSubscribeResponse
        {
            Position = position,
            TotalCount = position,
            Message = $"You are #{position} on the waitlist. We'll be in touch soon!"
        }, "Subscribed successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<int>> GetCountAsync()
    {
        var count = await _db.WaitlistEntries.CountAsync(w => !w.IsDeleted);
        return ApiResponse<int>.SuccessResponse(count);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<WaitlistEntryDto>>> GetAdminListAsync(int page, int pageSize, bool? isNotified)
    {
        var q = _db.WaitlistEntries.Where(w => !w.IsDeleted);
        if (isNotified.HasValue) q = q.Where(w => w.IsNotified == isNotified.Value);
        var total = await q.CountAsync();
        var items = await q.OrderBy(w => w.Position)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(w => new WaitlistEntryDto
        {
            Id = w.Id,
            Email = w.Email,
            Company = w.Company,
            Role = w.Role,
            InterestedPlan = w.InterestedPlan ?? w.Plan,
            Position = w.Position,
            IsNotified = w.IsNotified,
            NotifiedAt = w.NotifiedAt,
            CreatedAt = w.CreatedAt
        }).ToList();

        return ApiResponse<PagedResult<WaitlistEntryDto>>.SuccessResponse(
            PagedResult<WaitlistEntryDto>.Create(dtos, page, pageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WaitlistEntryDto>> NotifyAsync(Guid id)
    {
        var entry = await _db.WaitlistEntries.FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted);
        if (entry == null) return ApiResponse<WaitlistEntryDto>.FailResponse("Waitlist entry not found");

        entry.IsNotified = true;
        entry.NotifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<WaitlistEntryDto>.SuccessResponse(new WaitlistEntryDto
        {
            Id = entry.Id,
            Email = entry.Email,
            Company = entry.Company,
            Role = entry.Role,
            InterestedPlan = entry.InterestedPlan ?? entry.Plan,
            Position = entry.Position,
            IsNotified = entry.IsNotified,
            NotifiedAt = entry.NotifiedAt,
            CreatedAt = entry.CreatedAt
        });
    }
}
