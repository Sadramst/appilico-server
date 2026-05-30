using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Newsletter;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Newsletter subscription service implementation.</summary>
public class NewsletterService : INewsletterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(IUnitOfWork unitOfWork, ILogger<NewsletterService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> SubscribeAsync(NewsletterSubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _unitOfWork.NewsletterSubscribers.GetByEmailAsync(email);

        if (existing != null)
        {
            if (existing.IsActive)
                return ApiResponse<bool>.SuccessResponse(true, "Already subscribed");

            // Re-activate
            existing.IsActive = true;
            existing.UnsubscribedAt = null;
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Resubscribed successfully");
        }

        await _unitOfWork.NewsletterSubscribers.AddAsync(new NewsletterSubscriber
        {
            Email = email,
            Source = request.Source,
            IsActive = true,
            SubscribedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("[Newsletter] New subscriber: {Email}", email);

        return ApiResponse<bool>.SuccessResponse(true);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> UnsubscribeAsync(NewsletterUnsubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<bool>.FailResponse("Email is required");

        var email = request.Email.Trim().ToLowerInvariant();
        var subscriber = await _unitOfWork.NewsletterSubscribers.GetActiveByEmailAsync(email);

        if (subscriber == null)
            return ApiResponse<bool>.SuccessResponse(true); // Idempotent — not found or already unsubscribed

        subscriber.IsActive = false;
        subscriber.UnsubscribedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("[Newsletter] Unsubscribed: {Email}", email);
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
