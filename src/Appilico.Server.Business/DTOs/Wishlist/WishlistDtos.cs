namespace Appilico.Server.Business.DTOs.Wishlist;

/// <summary>DTO for wishlist item.</summary>
public class WishlistDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the product price.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the image URL.</summary>
    public string? ImageUrl { get; set; }
    /// <summary>Gets or sets when added.</summary>
    public DateTime AddedAt { get; set; }
}
