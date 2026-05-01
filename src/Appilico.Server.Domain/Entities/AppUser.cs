using Microsoft.AspNetCore.Identity;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Application user extending ASP.NET Identity.
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>Gets or sets the user's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the company or organisation name.</summary>
    public string? Company { get; set; }

    /// <summary>Gets or sets the subscription plan (Starter/Professional/Enterprise).</summary>
    public string SubscriptionPlan { get; set; } = "Starter";

    /// <summary>Gets or sets the user's avatar URL.</summary>
    public string? Avatar { get; set; }

    /// <summary>Gets or sets the Cloudinary public ID for the avatar.</summary>
    public string? CloudinaryPublicId { get; set; }

    /// <summary>Gets or sets the user's date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gets or sets whether the user account is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Navigation property for refresh tokens.</summary>
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>Navigation property for customer profile.</summary>
    public virtual Customer? Customer { get; set; }
}
