using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a customer profile linked to a user account.
/// </summary>
public class Customer : BaseAuditableEntity
{
    /// <summary>Gets or sets the associated user ID (FK).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique customer code.</summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the loyalty points balance.</summary>
    public int LoyaltyPoints { get; set; }

    /// <summary>Gets or sets the membership tier.</summary>
    public MembershipTier MembershipTier { get; set; } = MembershipTier.Bronze;

    /// <summary>Gets or sets the total purchase amount.</summary>
    public decimal TotalPurchases { get; set; }

    /// <summary>Gets or sets the date the customer joined.</summary>
    public DateTime JoinDate { get; set; }

    /// <summary>Navigation property for the user.</summary>
    public virtual AppUser User { get; set; } = null!;

    /// <summary>Navigation property for addresses.</summary>
    public virtual ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();

    /// <summary>Navigation property for orders.</summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>Navigation property for reviews.</summary>
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

    /// <summary>Navigation property for wishlists.</summary>
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    /// <summary>Navigation property for carts.</summary>
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    /// <summary>Navigation property for voucher redemptions.</summary>
    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
