using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Waitlist;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Waitlist service implementation.</summary>
public class WaitlistService : IWaitlistService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WaitlistService> _logger;
    private readonly IEmailWorkQueue _emailWorkQueue;

    public WaitlistService(IUnitOfWork unitOfWork, ILogger<WaitlistService> logger, IEmailWorkQueue emailWorkQueue)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailWorkQueue = emailWorkQueue;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<WaitlistSubscribeResponse>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.WaitlistEntries.GetByEmailAsync(email);
        var totalCount = await _unitOfWork.WaitlistEntries.CountActiveAsync();

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

        await _unitOfWork.WaitlistEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Waitlist entry added: {Email} at position {Position}", email, position);

        await _emailWorkQueue.QueueAsync((emailService, _) => emailService.SendWaitlistConfirmationAsync(email, position));

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
        var count = await _unitOfWork.WaitlistEntries.CountActiveAsync();
        return ApiResponse<int>.SuccessResponse(count);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<WaitlistEntryDto>>> GetAdminListAsync(int page, int pageSize, bool? isNotified)
    {
        var normalized = PaginationRequest.Normalize(page, pageSize, maxPageSize: 100);
        var (items, total) = await _unitOfWork.WaitlistEntries.GetAdminPageAsync(normalized.Page, normalized.PageSize, isNotified);

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
            PagedResult<WaitlistEntryDto>.Create(dtos, normalized.Page, normalized.PageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<WaitlistEntryDto>> NotifyAsync(Guid id)
    {
        var entry = await _unitOfWork.WaitlistEntries.GetActiveByIdAsync(id);
        if (entry == null) return ApiResponse<WaitlistEntryDto>.FailResponse("Waitlist entry not found");

        entry.IsNotified = true;
        entry.NotifiedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

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
