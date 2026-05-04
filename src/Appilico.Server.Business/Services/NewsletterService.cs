using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Newsletter;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Services;

/// <summary>Newsletter subscription service implementation.</summary>
public class NewsletterService : INewsletterService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(AppDbContext db, ILogger<NewsletterService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> SubscribeAsync(NewsletterSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _db.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == email);

        if (existing != null)
        {
            if (existing.IsActive)
                return ApiResponse<bool>.SuccessResponse(true, "Already subscribed");

            // Re-activate
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Resubscribed successfully");
        }

        _db.NewsletterSubscribers.Add(new NewsletterSubscriber
        {
            Email = email,
            Source = request.Source,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("[Newsletter] New subscriber: {Email}", email);

        return ApiResponse<bool>.SuccessResponse(true);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> UnsubscribeAsync(NewsletterUnsubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var subscriber = await _db.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == email && s.IsActive);

        if (subscriber == null)
            return ApiResponse<bool>.SuccessResponse(true); // Idempotent — not found or already unsubscribed

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("[Newsletter] Unsubscribed: {Email}", email);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
