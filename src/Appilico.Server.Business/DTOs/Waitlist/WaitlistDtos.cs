namespace Appilico.Server.Business.DTOs.Waitlist;

/// <summary>Request DTO for subscribing to the waitlist.</summary>
public class WaitlistSubscribeRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? Plan { get; set; }
    public string? InterestedPlan { get; set; }
}

/// <summary>Response DTO after waitlist subscribe.</summary>
public class WaitlistSubscribeResponse
{
    public int Position { get; set; }
    public int TotalCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>DTO for a waitlist entry (admin view).</summary>
public class WaitlistEntryDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Role { get; set; }
    public string? InterestedPlan { get; set; }
    public int Position { get; set; }
    public bool IsNotified { get; set; }
    public DateTime? NotifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
