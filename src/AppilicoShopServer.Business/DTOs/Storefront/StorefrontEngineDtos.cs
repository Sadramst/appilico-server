namespace AppilicoShopServer.Business.DTOs.Storefront;

/// <summary>Configurable shipping policy for the storefront engine.</summary>
public class StorefrontShippingConfigDto
{
    /// <summary>Gets or sets the shipping strategy: flat | free | threshold | weight.</summary>
    public string Strategy { get; set; } = "threshold";

    /// <summary>Gets or sets the flat shipping rate applied when not free.</summary>
    public decimal FlatRate { get; set; }

    /// <summary>Gets or sets the order subtotal at or above which shipping is free (0 disables).</summary>
    public decimal FreeShippingThreshold { get; set; }

    /// <summary>Gets or sets the per-kilogram rate for the weight strategy.</summary>
    public decimal PerKgRate { get; set; }

    /// <summary>Gets or sets an additional fixed handling fee.</summary>
    public decimal HandlingFee { get; set; }

    /// <summary>Gets or sets the shipping currency.</summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>Gets or sets the client-facing label for the shipping line.</summary>
    public string Label { get; set; } = "Standard Shipping";
}

/// <summary>Configurable tax policy for the storefront engine.</summary>
public class StorefrontTaxConfigDto
{
    /// <summary>Gets or sets the tax strategy: none | percentage | inclusive.</summary>
    public string Strategy { get; set; } = "percentage";

    /// <summary>Gets or sets the tax rate percentage.</summary>
    public decimal RatePercent { get; set; }

    /// <summary>Gets or sets whether catalog prices already include tax.</summary>
    public bool PricesIncludeTax { get; set; }

    /// <summary>Gets or sets whether tax also applies to shipping.</summary>
    public bool AppliesToShipping { get; set; }

    /// <summary>Gets or sets the client-facing tax label, e.g. GST or VAT.</summary>
    public string Label { get; set; } = "Tax";
}

/// <summary>A generic page-builder section descriptor.</summary>
public class StorefrontSectionDto
{
    /// <summary>Gets or sets the stable section ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the section type key clients render generically.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the section title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional section subtitle.</summary>
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the section is enabled.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Gets or sets the render order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets free-form section settings clients can interpret.</summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}

/// <summary>Writable storefront configuration document persisted per storefront key.</summary>
public class StorefrontEditableConfigDto
{
    /// <summary>Gets or sets the storefront key this document belongs to.</summary>
    public string StorefrontKey { get; set; } = "default";

    /// <summary>Gets or sets the store display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Gets or sets the store tagline.</summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>Gets or sets the logo URL.</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the favicon URL.</summary>
    public string FaviconUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the support email.</summary>
    public string SupportEmail { get; set; } = string.Empty;

    /// <summary>Gets or sets the support phone.</summary>
    public string SupportPhone { get; set; } = string.Empty;

    /// <summary>Gets or sets the store timezone.</summary>
    public string TimeZone { get; set; } = string.Empty;

    /// <summary>Gets or sets the default locale.</summary>
    public string DefaultLocale { get; set; } = string.Empty;

    /// <summary>Gets or sets the default currency.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the default country.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>Gets or sets the theme preset.</summary>
    public string ThemePreset { get; set; } = string.Empty;

    /// <summary>Gets or sets the layout preset.</summary>
    public string LayoutPreset { get; set; } = string.Empty;

    /// <summary>Gets or sets the product card style.</summary>
    public string ProductCardStyle { get; set; } = string.Empty;

    /// <summary>Gets or sets semantic color token overrides.</summary>
    public Dictionary<string, string> ColorTokens { get; set; } = new();

    /// <summary>Gets or sets typography token overrides.</summary>
    public Dictionary<string, string> TypographyTokens { get; set; } = new();

    /// <summary>Gets or sets spacing/radius token overrides.</summary>
    public Dictionary<string, string> SpacingTokens { get; set; } = new();

    /// <summary>Gets or sets the homepage layout sections.</summary>
    public List<StorefrontSectionDto> HomepageSections { get; set; } = new();

    /// <summary>Gets or sets feature capability overrides keyed by capability name.</summary>
    public Dictionary<string, bool> CapabilityOverrides { get; set; } = new();

    /// <summary>Gets or sets the shipping policy.</summary>
    public StorefrontShippingConfigDto Shipping { get; set; } = new();

    /// <summary>Gets or sets the tax policy.</summary>
    public StorefrontTaxConfigDto Tax { get; set; } = new();

    /// <summary>Gets or sets the default SEO title.</summary>
    public string SeoTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the default SEO description.</summary>
    public string SeoDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the default SEO keywords.</summary>
    public List<string> SeoKeywords { get; set; } = new();

    /// <summary>Gets or sets when this document was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>Gets or sets who last updated this document.</summary>
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>Request to compute a shipping quote.</summary>
public class ShippingQuoteRequestDto
{
    /// <summary>Gets or sets the order subtotal before shipping and tax.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Gets or sets the total item count.</summary>
    public int ItemCount { get; set; }

    /// <summary>Gets or sets the total cart weight in kilograms.</summary>
    public decimal? TotalWeightKg { get; set; }

    /// <summary>Gets or sets the destination country code.</summary>
    public string? CountryCode { get; set; }
}

/// <summary>Computed shipping quote result.</summary>
public class ShippingQuoteResultDto
{
    /// <summary>Gets or sets the strategy used.</summary>
    public string Strategy { get; set; } = string.Empty;

    /// <summary>Gets or sets the computed shipping cost.</summary>
    public decimal ShippingCost { get; set; }

    /// <summary>Gets or sets the handling fee component.</summary>
    public decimal HandlingFee { get; set; }

    /// <summary>Gets or sets the total shipping charge (shipping plus handling).</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets whether the order qualifies for free shipping.</summary>
    public bool FreeShippingEligible { get; set; }

    /// <summary>Gets or sets the configured free shipping threshold (0 if disabled).</summary>
    public decimal FreeShippingThreshold { get; set; }

    /// <summary>Gets or sets the amount remaining to reach free shipping (0 if eligible or disabled).</summary>
    public decimal AmountToFreeShipping { get; set; }

    /// <summary>Gets or sets the currency.</summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>Gets or sets the client-facing label.</summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>Request to compute a tax quote.</summary>
public class TaxQuoteRequestDto
{
    /// <summary>Gets or sets the taxable subtotal.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Gets or sets the shipping amount to consider when tax applies to shipping.</summary>
    public decimal ShippingAmount { get; set; }
}

/// <summary>Computed tax quote result.</summary>
public class TaxQuoteResultDto
{
    /// <summary>Gets or sets the strategy used.</summary>
    public string Strategy { get; set; } = string.Empty;

    /// <summary>Gets or sets the tax rate percentage applied.</summary>
    public decimal RatePercent { get; set; }

    /// <summary>Gets or sets the amount the tax was computed on.</summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>Gets or sets the computed tax amount.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Gets or sets whether prices already include tax.</summary>
    public bool PricesIncludeTax { get; set; }

    /// <summary>Gets or sets the total including tax (and shipping when provided).</summary>
    public decimal TotalWithTax { get; set; }

    /// <summary>Gets or sets the client-facing tax label.</summary>
    public string Label { get; set; } = string.Empty;
}

/// <summary>Page-builder homepage document for a storefront.</summary>
public class StorefrontHomePageDto
{
    /// <summary>Gets or sets the storefront key.</summary>
    public string StorefrontKey { get; set; } = "default";

    /// <summary>Gets or sets the ordered, enabled homepage sections.</summary>
    public List<StorefrontSectionDto> Sections { get; set; } = new();

    /// <summary>Gets or sets when this document was generated.</summary>
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>Summary of a registered storefront tenant.</summary>
public class StorefrontSummaryDto
{
    /// <summary>Gets or sets the storefront key.</summary>
    public string StorefrontKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the store display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Gets or sets the store tagline.</summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>Gets or sets the logo URL.</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this is the default storefront.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Gets or sets when the storefront was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>Request to create or update a storefront tenant.</summary>
public class StorefrontUpsertRequestDto
{
    /// <summary>Gets or sets the storefront key (slug). Required.</summary>
    public string StorefrontKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the store display name.</summary>
    public string StoreName { get; set; } = string.Empty;

    /// <summary>Gets or sets the store tagline.</summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>Gets or sets the logo URL.</summary>
    public string LogoUrl { get; set; } = string.Empty;
}
