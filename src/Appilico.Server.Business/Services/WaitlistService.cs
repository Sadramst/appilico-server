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

    public WaitlistService(AppDbContext db, ILogger<WaitlistService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<WaitlistSubscribeResponse>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _db.WaitlistEntries.FirstOrDefaultAsync(w => w.Email == email && !w.IsDeleted);
        if (existing != null)
        {
            return ApiResponse<WaitlistSubscribeResponse>.SuccessResponse(new WaitlistSubscribeResponse
            {
                Position = existing.Position,
                Message = $"You are already on the waitlist at position #{existing.Position}."
            }, "Already subscribed");
        }

        var count = await _db.WaitlistEntries.CountAsync(w => !w.IsDeleted);
        var position = count + 1;

        var entry = new WaitlistEntry
        {
            Email = email,
            Company = request.Company,
            Role = request.Role,
            Plan = request.Plan,
            Position = position
        };

        _db.WaitlistEntries.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Waitlist entry added: {Email} at position {Position}", email, position);

        return ApiResponse<WaitlistSubscribeResponse>.SuccessResponse(new WaitlistSubscribeResponse
        {
            Position = position,
            Message = $"You are #{ position} on the waitlist. We'll be in touch soon!"
        }, "Subscribed successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<int>> GetCountAsync()
    {
        var count = await _db.WaitlistEntries.CountAsync(w => !w.IsDeleted);
        return ApiResponse<int>.SuccessResponse(count);
    }
}
