namespace Appilico.Server.Business.DTOs.Contact;

/// <summary>Request DTO for submitting a contact form.</summary>
public class ContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
}
