namespace AppilicoShopServer.Business.Options;

/// <summary>Configurable public storefront metadata exposed to reusable clients.</summary>
public class StorefrontOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Storefront";

    /// <summary>Engine name shown to generic clients.</summary>
    public string EngineName { get; set; } = "AppilicoShopServer";

    /// <summary>Default store display name.</summary>
    public string StoreName { get; set; } = "Appilico Shop";

    /// <summary>Public API base path.</summary>
    public string ApiBasePath { get; set; } = "/api";

    /// <summary>Public server base URL when known.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;

    /// <summary>Default storefront currency.</summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>Default storefront country.</summary>
    public string Country { get; set; } = "AU";

    /// <summary>Default storefront locale.</summary>
    public string DefaultLocale { get; set; } = "en-AU";

    /// <summary>Locales supported by the storefront.</summary>
    public List<string> SupportedLocales { get; set; } = new() { "en-AU" };

    /// <summary>Default theme preset for clients.</summary>
    public string ThemePreset { get; set; } = "system";

    /// <summary>Whether anonymous checkout is available.</summary>
    public bool AllowGuestCheckout { get; set; } = true;

    /// <summary>Whether discounts are available.</summary>
    public bool EnableDiscounts { get; set; } = true;

    /// <summary>Whether vouchers are available.</summary>
    public bool EnableVouchers { get; set; } = true;

    /// <summary>Whether wishlist flows are available.</summary>
    public bool EnableWishlist { get; set; } = true;

    /// <summary>Whether product reviews are available.</summary>
    public bool EnableReviews { get; set; } = true;

    /// <summary>Whether blog/content pages are available.</summary>
    public bool EnableBlog { get; set; } = true;

    /// <summary>Whether newsletter signup is available.</summary>
    public bool EnableNewsletter { get; set; } = true;

    /// <summary>Whether subscription plans are available.</summary>
    public bool EnableSubscriptions { get; set; } = true;

    /// <summary>Whether visual/digital product flows are available.</summary>
    public bool EnableVisuals { get; set; } = true;

    /// <summary>Whether clients should send store context headers for future multi-store support.</summary>
    public bool EnableStoreContextHeaders { get; set; } = true;

    /// <summary>Navigation slots clients can render generically.</summary>
    public List<string> NavigationSlots { get; set; } = new() { "primary", "mobile", "footer", "account" };

    /// <summary>Product fields expected on public product cards.</summary>
    public List<string> ProductCardFields { get; set; } = new()
    {
        "id",
        "name",
        "sku",
        "basePrice",
        "primaryImageUrl",
        "averageRating",
        "totalReviews",
        "isFeatured",
        "stockQuantity"
    };
}
