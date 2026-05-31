namespace AppilicoShopServer.Business.Options;

/// <summary>Configurable public storefront metadata exposed to reusable clients.</summary>
public class StorefrontOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Storefront";

    /// <summary>Engine name shown to generic clients.</summary>
    public string EngineName { get; set; } = "AppilicoShopServer";

    /// <summary>Stable storefront key for current single-store mode and future multi-store routing.</summary>
    public string StorefrontKey { get; set; } = "default";

    /// <summary>Storefront mode exposed to generic clients.</summary>
    public string StorefrontMode { get; set; } = "single-store";

    /// <summary>Default store display name.</summary>
    public string StoreName { get; set; } = "Appilico Shop";

    /// <summary>Default store tagline.</summary>
    public string Tagline { get; set; } = "Reusable commerce for modern storefronts";

    /// <summary>Public logo URL.</summary>
    public string LogoUrl { get; set; } = string.Empty;

    /// <summary>Public favicon URL.</summary>
    public string FaviconUrl { get; set; } = string.Empty;

    /// <summary>Support email rendered by clients.</summary>
    public string SupportEmail { get; set; } = "support@appilico.com.au";

    /// <summary>Support phone rendered by clients.</summary>
    public string SupportPhone { get; set; } = string.Empty;

    /// <summary>Store timezone.</summary>
    public string TimeZone { get; set; } = "UTC";

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

    /// <summary>Default layout preset for clients.</summary>
    public string LayoutPreset { get; set; } = "commerce-standard";

    /// <summary>Default product card style.</summary>
    public string ProductCardStyle { get; set; } = "image-first";

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

    /// <summary>Store context header name.</summary>
    public string StoreContextHeaderName { get; set; } = "X-Storefront-Key";

    /// <summary>Store context resolution strategy.</summary>
    public string StoreContextResolutionStrategy { get; set; } = "single-store-default";

    /// <summary>Whether this deployment currently supports multiple stores.</summary>
    public bool SupportsMultiStore { get; set; }

    /// <summary>Default SEO title.</summary>
    public string DefaultSeoTitle { get; set; } = "Appilico Shop";

    /// <summary>Default SEO description.</summary>
    public string DefaultSeoDescription { get; set; } = "Reusable buy-and-sell storefront powered by AppilicoShopServer.";

    /// <summary>Default SEO keywords.</summary>
    public List<string> SeoKeywords { get; set; } = new() { "commerce", "storefront", "shop" };

    /// <summary>Navigation slots clients can render generically.</summary>
    public List<string> NavigationSlots { get; set; } = new() { "primary", "mobile", "footer", "account" };

    /// <summary>Navigation links clients can render generically.</summary>
    public List<StorefrontNavigationLinkOptions> NavigationLinks { get; set; } = new()
    {
        new() { Id = "home", Label = "Home", Path = "/", Slot = "primary", SortOrder = 10 },
        new() { Id = "catalog", Label = "Shop", Path = "/products", Slot = "primary", SortOrder = 20 },
        new() { Id = "categories", Label = "Categories", Path = "/categories", Slot = "primary", SortOrder = 30 },
        new() { Id = "offers", Label = "Offers", Path = "/offers", Slot = "primary", SortOrder = 40 },
        new() { Id = "account", Label = "Account", Path = "/account", Slot = "account", SortOrder = 10, RequiresAuth = true },
        new() { Id = "orders", Label = "Orders", Path = "/account/orders", Slot = "account", SortOrder = 20, RequiresAuth = true },
        new() { Id = "contact", Label = "Contact", Path = "/contact", Slot = "footer", SortOrder = 10 },
        new() { Id = "blog", Label = "Blog", Path = "/blog", Slot = "footer", SortOrder = 20 }
    };

    /// <summary>Public social links.</summary>
    public List<StorefrontLinkOptions> SocialLinks { get; set; } = new();

    /// <summary>Public legal links.</summary>
    public List<StorefrontLinkOptions> LegalLinks { get; set; } = new()
    {
        new() { Id = "privacy", Label = "Privacy", Url = "/privacy" },
        new() { Id = "terms", Label = "Terms", Url = "/terms" },
        new() { Id = "returns", Label = "Returns", Url = "/returns" }
    };

    /// <summary>Semantic color tokens for generic clients.</summary>
    public Dictionary<string, string> ThemeColors { get; set; } = new()
    {
        ["background"] = "#f8fafc",
        ["surface"] = "#ffffff",
        ["text"] = "#111827",
        ["mutedText"] = "#64748b",
        ["primary"] = "#2563eb",
        ["primaryText"] = "#ffffff",
        ["accent"] = "#14b8a6",
        ["border"] = "#e2e8f0",
        ["success"] = "#16a34a",
        ["warning"] = "#f59e0b",
        ["danger"] = "#dc2626"
    };

    /// <summary>Typography tokens for generic clients.</summary>
    public Dictionary<string, string> TypographyTokens { get; set; } = new()
    {
        ["headingFont"] = "Inter, system-ui, sans-serif",
        ["bodyFont"] = "Inter, system-ui, sans-serif",
        ["baseFontSize"] = "16px",
        ["headingWeight"] = "700",
        ["bodyWeight"] = "400"
    };

    /// <summary>Spacing and radius tokens for generic clients.</summary>
    public Dictionary<string, string> SpacingTokens { get; set; } = new()
    {
        ["containerMaxWidth"] = "1200px",
        ["sectionGap"] = "48px",
        ["cardRadius"] = "8px",
        ["buttonRadius"] = "6px"
    };

    /// <summary>Homepage section keys clients can render in order.</summary>
    public List<string> HomepageSections { get; set; } = new()
    {
        "hero",
        "featuredProducts",
        "categoryRail",
        "offers",
        "newsletter"
    };

    /// <summary>Shipping strategy key for clients.</summary>
    public string ShippingStrategy { get; set; } = "configured-by-server";

    /// <summary>Tax strategy key for clients.</summary>
    public string TaxStrategy { get; set; } = "configured-by-server";

    /// <summary>Default shipping policy used when no persisted override exists.</summary>
    public StorefrontShippingPolicyOptions Shipping { get; set; } = new();

    /// <summary>Default tax policy used when no persisted override exists.</summary>
    public StorefrontTaxPolicyOptions Tax { get; set; } = new();

    /// <summary>Returns policy URL or path.</summary>
    public string ReturnsPolicyUrl { get; set; } = "/returns";

    /// <summary>Terms URL or path.</summary>
    public string TermsUrl { get; set; } = "/terms";

    /// <summary>Privacy URL or path.</summary>
    public string PrivacyUrl { get; set; } = "/privacy";

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

/// <summary>Configurable public link metadata.</summary>
public class StorefrontLinkOptions
{
    /// <summary>Stable link ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Link label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>URL or route path.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Optional icon key.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Whether clients should open the link in a new tab.</summary>
    public bool OpenInNewTab { get; set; }
}

/// <summary>Configurable navigation link metadata.</summary>
public class StorefrontNavigationLinkOptions
{
    /// <summary>Stable link ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Link label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Client route path.</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Navigation slot.</summary>
    public string Slot { get; set; } = string.Empty;

    /// <summary>Sort order inside the slot.</summary>
    public int SortOrder { get; set; }

    /// <summary>Whether auth is required to render or follow the link.</summary>
    public bool RequiresAuth { get; set; }

    /// <summary>Optional required role.</summary>
    public string RequiredRole { get; set; } = string.Empty;
}

/// <summary>Default shipping policy options for the storefront engine.</summary>
public class StorefrontShippingPolicyOptions
{
    /// <summary>Shipping strategy: flat | free | threshold | weight.</summary>
    public string Strategy { get; set; } = "threshold";

    /// <summary>Flat shipping rate applied when not free.</summary>
    public decimal FlatRate { get; set; } = 9.99m;

    /// <summary>Subtotal at or above which shipping is free (0 disables).</summary>
    public decimal FreeShippingThreshold { get; set; } = 80m;

    /// <summary>Per-kilogram rate for the weight strategy.</summary>
    public decimal PerKgRate { get; set; } = 0m;

    /// <summary>Additional fixed handling fee.</summary>
    public decimal HandlingFee { get; set; } = 0m;

    /// <summary>Shipping currency.</summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>Client-facing shipping label.</summary>
    public string Label { get; set; } = "Standard Shipping";
}

/// <summary>Default tax policy options for the storefront engine.</summary>
public class StorefrontTaxPolicyOptions
{
    /// <summary>Tax strategy: none | percentage | inclusive.</summary>
    public string Strategy { get; set; } = "percentage";

    /// <summary>Tax rate percentage.</summary>
    public decimal RatePercent { get; set; } = 10m;

    /// <summary>Whether catalog prices already include tax.</summary>
    public bool PricesIncludeTax { get; set; }

    /// <summary>Whether tax also applies to shipping.</summary>
    public bool AppliesToShipping { get; set; }

    /// <summary>Client-facing tax label.</summary>
    public string Label { get; set; } = "GST";
}
