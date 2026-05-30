namespace AppilicoShopServer.Business.DTOs.Storefront;

/// <summary>Public storefront bootstrap contract for reusable clients.</summary>
public class StorefrontConfigDto
{
    /// <summary>Gets or sets the engine name.</summary>
    public string EngineName { get; set; } = string.Empty;

    /// <summary>Gets or sets the API version.</summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>Gets or sets store metadata.</summary>
    public StorefrontBrandDto Brand { get; set; } = new();

    /// <summary>Gets or sets locale and currency metadata.</summary>
    public StorefrontLocaleDto Locale { get; set; } = new();

    /// <summary>Gets or sets theme metadata.</summary>
    public StorefrontThemeDto Theme { get; set; } = new();

    /// <summary>Gets or sets feature capabilities.</summary>
    public StorefrontCapabilitiesDto Capabilities { get; set; } = new();

    /// <summary>Gets or sets route contracts available to the client.</summary>
    public List<StorefrontEndpointDto> Endpoints { get; set; } = new();

    /// <summary>Gets or sets navigation metadata.</summary>
    public StorefrontNavigationDto Navigation { get; set; } = new();

    /// <summary>Gets or sets checkout policy metadata.</summary>
    public StorefrontCheckoutPolicyDto Checkout { get; set; } = new();

    /// <summary>Gets or sets reusable auth metadata.</summary>
    public StorefrontAuthContractDto Auth { get; set; } = new();

    /// <summary>Gets or sets when this config was generated.</summary>
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>Store brand metadata.</summary>
public class StorefrontBrandDto
{
    /// <summary>Gets or sets store display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Gets or sets public server URL.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}

/// <summary>Locale metadata.</summary>
public class StorefrontLocaleDto
{
    /// <summary>Gets or sets default locale.</summary>
    public string DefaultLocale { get; set; } = string.Empty;

    /// <summary>Gets or sets supported locales.</summary>
    public List<string> SupportedLocales { get; set; } = new();

    /// <summary>Gets or sets default currency.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets default country.</summary>
    public string Country { get; set; } = string.Empty;
}

/// <summary>Theme metadata.</summary>
public class StorefrontThemeDto
{
    /// <summary>Gets or sets default theme preset.</summary>
    public string Preset { get; set; } = string.Empty;

    /// <summary>Gets or sets product card fields clients should expect.</summary>
    public List<string> ProductCardFields { get; set; } = new();
}

/// <summary>Feature capability metadata.</summary>
public class StorefrontCapabilitiesDto
{
    /// <summary>Gets or sets whether catalog browsing is available.</summary>
    public bool ProductCatalog { get; set; }

    /// <summary>Gets or sets whether nested categories are available.</summary>
    public bool CategoryTree { get; set; }

    /// <summary>Gets or sets whether brands are available.</summary>
    public bool Brands { get; set; }

    /// <summary>Gets or sets whether cart flows are available.</summary>
    public bool Cart { get; set; }

    /// <summary>Gets or sets whether customer accounts are available.</summary>
    public bool CustomerAccounts { get; set; }

    /// <summary>Gets or sets whether orders are available.</summary>
    public bool Orders { get; set; }

    /// <summary>Gets or sets whether payments are available.</summary>
    public bool Payments { get; set; }

    /// <summary>Gets or sets whether discounts are available.</summary>
    public bool Discounts { get; set; }

    /// <summary>Gets or sets whether vouchers are available.</summary>
    public bool Vouchers { get; set; }

    /// <summary>Gets or sets whether wishlist is available.</summary>
    public bool Wishlist { get; set; }

    /// <summary>Gets or sets whether product reviews are available.</summary>
    public bool Reviews { get; set; }

    /// <summary>Gets or sets whether subscriptions are available.</summary>
    public bool Subscriptions { get; set; }

    /// <summary>Gets or sets whether blog/content is available.</summary>
    public bool Blog { get; set; }

    /// <summary>Gets or sets whether newsletter signup is available.</summary>
    public bool Newsletter { get; set; }

    /// <summary>Gets or sets whether digital visuals are available.</summary>
    public bool Visuals { get; set; }

    /// <summary>Gets or sets whether clients should send future store context headers.</summary>
    public bool StoreContextHeaders { get; set; }
}

/// <summary>API endpoint metadata.</summary>
public class StorefrontEndpointDto
{
    /// <summary>Gets or sets stable endpoint ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets HTTP method.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Gets or sets route path.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Gets or sets whether authentication is required.</summary>
    public bool RequiresAuth { get; set; }

    /// <summary>Gets or sets intended client use.</summary>
    public string UseCase { get; set; } = string.Empty;
}

/// <summary>Navigation metadata.</summary>
public class StorefrontNavigationDto
{
    /// <summary>Gets or sets supported navigation slots.</summary>
    public List<string> Slots { get; set; } = new();

    /// <summary>Gets or sets category tree endpoint ID.</summary>
    public string CategoryTreeEndpointId { get; set; } = "categories.tree";
}

/// <summary>Checkout policy metadata.</summary>
public class StorefrontCheckoutPolicyDto
{
    /// <summary>Gets or sets whether guest checkout is allowed.</summary>
    public bool AllowGuestCheckout { get; set; }

    /// <summary>Gets or sets cart endpoint ID.</summary>
    public string CartEndpointId { get; set; } = "cart.current";

    /// <summary>Gets or sets order creation endpoint ID.</summary>
    public string CreateOrderEndpointId { get; set; } = "orders.create";

    /// <summary>Gets or sets payment creation endpoint ID.</summary>
    public string CreatePaymentEndpointId { get; set; } = "payments.create";
}

/// <summary>Authentication metadata for generic clients.</summary>
public class StorefrontAuthContractDto
{
    /// <summary>Gets or sets token scheme.</summary>
    public string TokenScheme { get; set; } = "Bearer";

    /// <summary>Gets or sets customer role.</summary>
    public string CustomerRole { get; set; } = "Customer";

    /// <summary>Gets or sets privileged roles.</summary>
    public List<string> PrivilegedRoles { get; set; } = new();
}
