namespace Appilico.Server.Business.DTOs.Waitlist;

/// <summary>Request DTO for subscribing to the waitlist.</summary>
public class WaitlistSubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? Plan { get; set; }
}

/// <summary>Response DTO after waitlist subscribe.</summary>
public class WaitlistSubscribeResponse
{
    public int Position { get; set; }
    public string Message { get; set; } = string.Empty;
}
