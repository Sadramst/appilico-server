using Microsoft.AspNetCore.Identity;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Application role extending ASP.NET Identity.
/// </summary>
public class AppRole : IdentityRole
{
    /// <summary>Gets or sets the role description.</summary>
    public string? Description { get; set; }
}
