using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.DTOs.Customer;

/// <summary>DTO for customer.</summary>
public class CustomerDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the user ID.</summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>Gets or sets the customer code.</summary>
    public string CustomerCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email.</summary>
    public string? Email { get; set; }
    /// <summary>Gets or sets the phone.</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the loyalty points.</summary>
    public int LoyaltyPoints { get; set; }
    /// <summary>Gets or sets the membership tier.</summary>
    public MembershipTier MembershipTier { get; set; }
    /// <summary>Gets or sets the total purchases.</summary>
    public decimal TotalPurchases { get; set; }
    /// <summary>Gets or sets the join date.</summary>
    public DateTime JoinDate { get; set; }
    /// <summary>Gets or sets the addresses.</summary>
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}

/// <summary>DTO for customer address.</summary>
public class CustomerAddressDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets address line 1.</summary>
    public string AddressLine1 { get; set; } = string.Empty;
    /// <summary>Gets or sets address line 2.</summary>
    public string? AddressLine2 { get; set; }
    /// <summary>Gets or sets the city.</summary>
    public string City { get; set; } = string.Empty;
    /// <summary>Gets or sets the state.</summary>
    public string? State { get; set; }
    /// <summary>Gets or sets the postal code.</summary>
    public string PostalCode { get; set; } = string.Empty;
    /// <summary>Gets or sets the country.</summary>
    public string Country { get; set; } = string.Empty;
    /// <summary>Gets or sets whether this is default.</summary>
    public bool IsDefault { get; set; }
    /// <summary>Gets or sets the address type.</summary>
    public AddressType AddressType { get; set; }
}

/// <summary>DTO for creating a customer.</summary>
public class CreateCustomerRequest
{
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the phone.</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the password.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>DTO for updating a customer.</summary>
public class UpdateCustomerRequest
{
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the phone.</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the membership tier.</summary>
    public MembershipTier? MembershipTier { get; set; }
}

/// <summary>DTO for customer loyalty info.</summary>
public class CustomerLoyaltyDto
{
    /// <summary>Gets or sets the customer ID.</summary>
    public Guid CustomerId { get; set; }
    /// <summary>Gets or sets the loyalty points.</summary>
    public int LoyaltyPoints { get; set; }
    /// <summary>Gets or sets the membership tier.</summary>
    public MembershipTier MembershipTier { get; set; }
    /// <summary>Gets or sets the total purchases.</summary>
    public decimal TotalPurchases { get; set; }
}
