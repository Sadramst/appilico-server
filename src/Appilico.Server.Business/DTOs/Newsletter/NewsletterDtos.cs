namespace Appilico.Server.Business.DTOs.Newsletter;

/// <summary>Request DTO for subscribing to the newsletter.</summary>
public class NewsletterSubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Source { get; set; }
}

/// <summary>Request DTO for unsubscribing from the newsletter.</summary>
public class NewsletterUnsubscribeRequest
{
    public string Email { get; set; } = string.Empty;
}
