using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Newsletter;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Newsletter subscription service interface.</summary>
public interface INewsletterService
{
    Task<ApiResponse<bool>> SubscribeAsync(NewsletterSubscribeRequest request);
    Task<ApiResponse<bool>> UnsubscribeAsync(NewsletterUnsubscribeRequest request);
}
