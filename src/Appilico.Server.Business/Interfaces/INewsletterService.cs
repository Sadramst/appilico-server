using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Newsletter;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Newsletter subscription service interface.</summary>
public interface INewsletterService
{
    Task<ApiResponse<bool>> SubscribeAsync(NewsletterSubscribeRequest request);
    Task<ApiResponse<bool>> UnsubscribeAsync(NewsletterUnsubscribeRequest request);
}
