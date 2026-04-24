using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a special offer campaign.
/// </summary>
public class SpecialOffer : BaseAuditableEntity
{
    /// <summary>Gets or sets the offer name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the offer description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the banner image URL.</summary>
    public string? BannerImageUrl { get; set; }

    /// <summary>Gets or sets the offer type.</summary>
    public OfferType OfferType { get; set; }

    /// <summary>Gets or sets the offer start date.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the offer end date.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Gets or sets whether the offer is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property for offer products.</summary>
    public virtual ICollection<SpecialOfferProduct> SpecialOfferProducts { get; set; } = new List<SpecialOfferProduct>();
}
