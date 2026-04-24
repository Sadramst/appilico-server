using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a customer's address.
/// </summary>
public class CustomerAddress : BaseAuditableEntity
{
    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the address title (e.g. Home, Office).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the first line of the address.</summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>Gets or sets the second line of the address.</summary>
    public string? AddressLine2 { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Gets or sets the state or province.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets the postal code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the country.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this is the default address.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets the address type.</summary>
    public AddressType AddressType { get; set; }

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer Customer { get; set; } = null!;
}
