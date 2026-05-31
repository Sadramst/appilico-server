using System.Text.Json;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Storefront;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Domain.Constants;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace AppilicoShopServer.Business.Services;

/// <summary>Builds and persists storefront engine metadata for generic shop clients.</summary>
public class StorefrontService : IStorefrontService
{
    private const string ConfigGroup = "Storefront";
    private const string RegistryKey = "storefront:registry";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly StorefrontOptions _options;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Initializes the storefront service.</summary>
    public StorefrontService(IOptions<StorefrontOptions> options, IUnitOfWork unitOfWork)
    {
        _options = options.Value;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontConfigDto>> GetConfigAsync(string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);
        return ApiResponse<StorefrontConfigDto>.SuccessResponse(BuildConfig(key, effective));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontThemeDto>> GetThemeAsync(string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);
        return ApiResponse<StorefrontThemeDto>.SuccessResponse(BuildTheme(effective));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontEditableConfigDto>> GetEditableConfigAsync(string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);
        return ApiResponse<StorefrontEditableConfigDto>.SuccessResponse(effective);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontEditableConfigDto>> UpdateConfigAsync(StorefrontEditableConfigDto config, string updatedBy, string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (config == null)
            return ApiResponse<StorefrontEditableConfigDto>.FailResponse("Configuration payload is required.");

        var key = ResolveKey(storefrontKey ?? config.StorefrontKey);
        config.StorefrontKey = key;
        config.UpdatedAtUtc = DateTime.UtcNow;
        config.UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;

        await SaveEditableAsync(config, cancellationToken);

        var effective = await GetEffectiveConfigAsync(key);
        return ApiResponse<StorefrontEditableConfigDto>.SuccessResponse(effective, "Storefront configuration saved.");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ShippingQuoteResultDto>> GetShippingQuoteAsync(ShippingQuoteRequestDto request, string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request == null)
            return ApiResponse<ShippingQuoteResultDto>.FailResponse("Shipping quote request is required.");
        if (request.Subtotal < 0)
            return ApiResponse<ShippingQuoteResultDto>.FailResponse("Subtotal cannot be negative.");

        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);
        var shipping = effective.Shipping;

        var threshold = shipping.FreeShippingThreshold;
        var freeEligible = threshold > 0 && request.Subtotal >= threshold;
        decimal baseCost = shipping.Strategy?.ToLowerInvariant() switch
        {
            "free" => 0m,
            "weight" => Math.Round(Math.Max(0, request.TotalWeightKg ?? 0m) * shipping.PerKgRate, 2),
            _ => shipping.FlatRate
        };

        if (freeEligible)
            baseCost = 0m;

        var handling = freeEligible ? 0m : Math.Max(0, shipping.HandlingFee);
        var amountToFree = threshold > 0 && !freeEligible ? Math.Round(threshold - request.Subtotal, 2) : 0m;

        var result = new ShippingQuoteResultDto
        {
            Strategy = string.IsNullOrWhiteSpace(shipping.Strategy) ? "threshold" : shipping.Strategy,
            ShippingCost = Math.Max(0, baseCost),
            HandlingFee = handling,
            Total = Math.Max(0, baseCost) + handling,
            FreeShippingEligible = freeEligible,
            FreeShippingThreshold = threshold,
            AmountToFreeShipping = amountToFree < 0 ? 0m : amountToFree,
            Currency = NormalizeString(shipping.Currency, _options.Currency),
            Label = NormalizeString(shipping.Label, "Standard Shipping")
        };

        return ApiResponse<ShippingQuoteResultDto>.SuccessResponse(result);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<TaxQuoteResultDto>> GetTaxQuoteAsync(TaxQuoteRequestDto request, string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request == null)
            return ApiResponse<TaxQuoteResultDto>.FailResponse("Tax quote request is required.");
        if (request.Subtotal < 0)
            return ApiResponse<TaxQuoteResultDto>.FailResponse("Subtotal cannot be negative.");

        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);
        var tax = effective.Tax;

        var strategy = string.IsNullOrWhiteSpace(tax.Strategy) ? "percentage" : tax.Strategy.ToLowerInvariant();
        var rate = Math.Max(0, tax.RatePercent);
        var shippingAmount = Math.Max(0, request.ShippingAmount);
        var taxableBase = request.Subtotal + (tax.AppliesToShipping ? shippingAmount : 0m);

        decimal taxAmount;
        decimal totalWithTax;
        switch (strategy)
        {
            case "none":
                taxAmount = 0m;
                totalWithTax = request.Subtotal + shippingAmount;
                break;
            case "inclusive":
                // Price already includes tax; extract the embedded tax component.
                taxAmount = rate > 0 ? Math.Round(taxableBase - taxableBase / (1 + rate / 100m), 2) : 0m;
                totalWithTax = request.Subtotal + shippingAmount;
                break;
            default: // percentage (exclusive)
                taxAmount = Math.Round(taxableBase * rate / 100m, 2);
                totalWithTax = request.Subtotal + shippingAmount + taxAmount;
                break;
        }

        var result = new TaxQuoteResultDto
        {
            Strategy = strategy,
            RatePercent = rate,
            TaxableAmount = taxableBase,
            TaxAmount = taxAmount,
            PricesIncludeTax = strategy == "inclusive" || tax.PricesIncludeTax,
            TotalWithTax = totalWithTax,
            Label = NormalizeString(tax.Label, "Tax")
        };

        return ApiResponse<TaxQuoteResultDto>.SuccessResponse(result);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontHomePageDto>> GetHomePageAsync(string? storefrontKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ResolveKey(storefrontKey);
        var effective = await GetEffectiveConfigAsync(key);

        var sections = effective.HomepageSections
            .Where(s => s.Enabled)
            .OrderBy(s => s.SortOrder)
            .ToList();

        var page = new StorefrontHomePageDto
        {
            StorefrontKey = key,
            Sections = sections,
            GeneratedAtUtc = DateTime.UtcNow
        };

        return ApiResponse<StorefrontHomePageDto>.SuccessResponse(page);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<StorefrontSummaryDto>>> ListStorefrontsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var defaultKey = ResolveKey(null);
        var keys = await GetRegistryKeysAsync();
        if (!keys.Contains(defaultKey, StringComparer.OrdinalIgnoreCase))
            keys.Insert(0, defaultKey);

        var summaries = new List<StorefrontSummaryDto>();
        foreach (var key in keys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var effective = await GetEffectiveConfigAsync(key);
            summaries.Add(new StorefrontSummaryDto
            {
                StorefrontKey = key,
                StoreName = effective.StoreName,
                Tagline = effective.Tagline,
                LogoUrl = effective.LogoUrl,
                IsDefault = string.Equals(key, defaultKey, StringComparison.OrdinalIgnoreCase),
                UpdatedAtUtc = effective.UpdatedAtUtc
            });
        }

        return ApiResponse<List<StorefrontSummaryDto>>.SuccessResponse(summaries);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<StorefrontSummaryDto>> UpsertStorefrontAsync(StorefrontUpsertRequestDto request, string updatedBy, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request == null || string.IsNullOrWhiteSpace(request.StorefrontKey))
            return ApiResponse<StorefrontSummaryDto>.FailResponse("Storefront key is required.");

        var key = ResolveKey(request.StorefrontKey);
        var effective = await GetEffectiveConfigAsync(key);

        if (!string.IsNullOrWhiteSpace(request.StoreName)) effective.StoreName = request.StoreName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Tagline)) effective.Tagline = request.Tagline.Trim();
        if (!string.IsNullOrWhiteSpace(request.LogoUrl)) effective.LogoUrl = request.LogoUrl.Trim();

        effective.StorefrontKey = key;
        effective.UpdatedAtUtc = DateTime.UtcNow;
        effective.UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;

        await SaveEditableAsync(effective, cancellationToken);

        var summary = new StorefrontSummaryDto
        {
            StorefrontKey = key,
            StoreName = effective.StoreName,
            Tagline = effective.Tagline,
            LogoUrl = effective.LogoUrl,
            IsDefault = string.Equals(key, ResolveKey(null), StringComparison.OrdinalIgnoreCase),
            UpdatedAtUtc = effective.UpdatedAtUtc
        };

        return ApiResponse<StorefrontSummaryDto>.SuccessResponse(summary, "Storefront saved.");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteStorefrontAsync(string storefrontKey, string updatedBy, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = ResolveKey(storefrontKey);
        if (string.Equals(key, ResolveKey(null), StringComparison.OrdinalIgnoreCase))
            return ApiResponse<bool>.FailResponse("The default storefront cannot be deleted.");

        var setting = await _unitOfWork.Settings.GetByKeyAsync(ConfigKey(key));
        if (setting != null && !setting.IsDeleted)
            _unitOfWork.Settings.SoftDelete(setting);

        var keys = await GetRegistryKeysAsync();
        keys.RemoveAll(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
        await PersistRegistryAsync(keys, updatedBy);

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Storefront removed.");
    }

    // ----- Persistence helpers -----

    private static string ConfigKey(string key) => $"storefront:{key}:config";

    private string ResolveKey(string? storefrontKey)
    {
        var fallback = NormalizeString(_options.StorefrontKey, "default");
        var value = string.IsNullOrWhiteSpace(storefrontKey) ? fallback : storefrontKey;
        return value.Trim().ToLowerInvariant();
    }

    private async Task<StorefrontEditableConfigDto> GetEffectiveConfigAsync(string key)
    {
        var defaults = BuildDefaultEditableConfig(key);
        var setting = await _unitOfWork.Settings.GetByKeyAsync(ConfigKey(key));
        if (setting == null || setting.IsDeleted || string.IsNullOrWhiteSpace(setting.Value))
            return defaults;

        StorefrontEditableConfigDto? stored;
        try
        {
            stored = JsonSerializer.Deserialize<StorefrontEditableConfigDto>(setting.Value, JsonOptions);
        }
        catch (JsonException)
        {
            return defaults;
        }

        if (stored == null)
            return defaults;

        return MergeEditable(defaults, stored, key);
    }

    private async Task SaveEditableAsync(StorefrontEditableConfigDto config, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        var settingKey = ConfigKey(config.StorefrontKey);
        var existing = await _unitOfWork.Settings.GetByKeyAsync(settingKey);
        if (existing == null)
        {
            await _unitOfWork.Settings.AddAsync(new AppSetting
            {
                Key = settingKey,
                Value = json,
                Group = ConfigGroup,
                Description = $"Storefront configuration document for '{config.StorefrontKey}'.",
                CreatedBy = config.UpdatedBy
            });
        }
        else
        {
            existing.Value = json;
            existing.Group = ConfigGroup;
            existing.IsDeleted = false;
            existing.UpdatedBy = config.UpdatedBy;
            _unitOfWork.Settings.Update(existing);
        }

        var keys = await GetRegistryKeysAsync();
        if (!keys.Contains(config.StorefrontKey, StringComparer.OrdinalIgnoreCase))
        {
            keys.Add(config.StorefrontKey);
            await PersistRegistryAsync(keys, config.UpdatedBy);
        }

        await _unitOfWork.SaveChangesAsync();
        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task<List<string>> GetRegistryKeysAsync()
    {
        var setting = await _unitOfWork.Settings.GetByKeyAsync(RegistryKey);
        if (setting == null || setting.IsDeleted || string.IsNullOrWhiteSpace(setting.Value))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(setting.Value, JsonOptions) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private async Task PersistRegistryAsync(List<string> keys, string updatedBy)
    {
        var json = JsonSerializer.Serialize(keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), JsonOptions);
        var setting = await _unitOfWork.Settings.GetByKeyAsync(RegistryKey);
        if (setting == null)
        {
            await _unitOfWork.Settings.AddAsync(new AppSetting
            {
                Key = RegistryKey,
                Value = json,
                Group = ConfigGroup,
                Description = "Registry of known storefront keys.",
                CreatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy
            });
        }
        else
        {
            setting.Value = json;
            setting.Group = ConfigGroup;
            setting.IsDeleted = false;
            setting.UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;
            _unitOfWork.Settings.Update(setting);
        }
    }

    // ----- Builders -----

    private StorefrontConfigDto BuildConfig(string key, StorefrontEditableConfigDto effective)
    {
        var apiBasePath = NormalizeApiBasePath(_options.ApiBasePath);
        return new StorefrontConfigDto
        {
            EngineName = _options.EngineName,
            ApiVersion = "v1",
            StorefrontKey = key,
            StorefrontMode = NormalizeString(_options.StorefrontMode, "single-store"),
            Brand = new StorefrontBrandDto
            {
                StoreName = effective.StoreName,
                Tagline = effective.Tagline,
                LogoUrl = effective.LogoUrl,
                FaviconUrl = effective.FaviconUrl,
                PublicBaseUrl = _options.PublicBaseUrl,
                SupportEmail = effective.SupportEmail,
                SupportPhone = effective.SupportPhone,
                TimeZone = NormalizeString(effective.TimeZone, "UTC"),
                SocialLinks = BuildLinks(_options.SocialLinks),
                LegalLinks = BuildLinks(_options.LegalLinks)
            },
            Locale = new StorefrontLocaleDto
            {
                DefaultLocale = effective.DefaultLocale,
                SupportedLocales = BuildSupportedLocales(effective.DefaultLocale),
                Currency = effective.Currency,
                Country = effective.Country
            },
            Theme = BuildTheme(effective),
            Capabilities = BuildCapabilities(effective),
            Endpoints = BuildEndpoints(apiBasePath),
            Navigation = new StorefrontNavigationDto
            {
                Slots = NormalizeList(_options.NavigationSlots),
                CategoryTreeEndpointId = "categories.tree",
                Links = BuildNavigationLinks(_options.NavigationLinks)
            },
            Checkout = new StorefrontCheckoutPolicyDto
            {
                AllowGuestCheckout = _options.AllowGuestCheckout,
                CartEndpointId = "cart.current",
                CreateOrderEndpointId = "orders.create",
                CreatePaymentEndpointId = "payments.create",
                ShippingStrategy = NormalizeString(effective.Shipping.Strategy, _options.ShippingStrategy),
                TaxStrategy = NormalizeString(effective.Tax.Strategy, _options.TaxStrategy),
                Shipping = CloneShipping(effective.Shipping),
                Tax = CloneTax(effective.Tax),
                ReturnsPolicyUrl = _options.ReturnsPolicyUrl,
                TermsUrl = _options.TermsUrl,
                PrivacyUrl = _options.PrivacyUrl
            },
            Auth = new StorefrontAuthContractDto
            {
                TokenScheme = "Bearer",
                CustomerRole = AppConstants.Roles.Customer,
                PrivilegedRoles = new List<string>
                {
                    AppConstants.Roles.SuperAdmin,
                    AppConstants.Roles.Admin,
                    AppConstants.Roles.Manager
                }
            },
            Context = new StorefrontContextDto
            {
                DefaultStorefrontKey = ResolveKey(null),
                HeaderName = NormalizeString(_options.StoreContextHeaderName, "X-Storefront-Key"),
                ResolutionStrategy = NormalizeString(_options.StoreContextResolutionStrategy, "single-store-default"),
                SupportsMultiStore = _options.SupportsMultiStore
            },
            Seo = new StorefrontSeoDto
            {
                DefaultTitle = effective.SeoTitle,
                DefaultDescription = effective.SeoDescription,
                Keywords = NormalizeList(effective.SeoKeywords)
            },
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    private StorefrontThemeDto BuildTheme(StorefrontEditableConfigDto effective)
    {
        return new StorefrontThemeDto
        {
            Preset = effective.ThemePreset,
            LayoutPreset = effective.LayoutPreset,
            ProductCardStyle = effective.ProductCardStyle,
            ColorTokens = CopyDictionary(effective.ColorTokens),
            TypographyTokens = CopyDictionary(effective.TypographyTokens),
            SpacingTokens = CopyDictionary(effective.SpacingTokens),
            ProductCardFields = NormalizeList(_options.ProductCardFields),
            HomepageSections = effective.HomepageSections.Select(s => s.Type).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            HomepageLayout = effective.HomepageSections.OrderBy(s => s.SortOrder).ToList()
        };
    }

    private StorefrontCapabilitiesDto BuildCapabilities(StorefrontEditableConfigDto effective)
    {
        var overrides = effective.CapabilityOverrides ?? new Dictionary<string, bool>();
        bool Cap(string name, bool fallback) => overrides.TryGetValue(name, out var value) ? value : fallback;

        return new StorefrontCapabilitiesDto
        {
            ProductCatalog = Cap("productCatalog", true),
            CategoryTree = Cap("categoryTree", true),
            Brands = Cap("brands", true),
            Cart = Cap("cart", true),
            CustomerAccounts = Cap("customerAccounts", true),
            Orders = Cap("orders", true),
            Payments = Cap("payments", true),
            Discounts = Cap("discounts", _options.EnableDiscounts),
            Vouchers = Cap("vouchers", _options.EnableVouchers),
            Wishlist = Cap("wishlist", _options.EnableWishlist),
            Reviews = Cap("reviews", _options.EnableReviews),
            Subscriptions = Cap("subscriptions", _options.EnableSubscriptions),
            Blog = Cap("blog", _options.EnableBlog),
            Newsletter = Cap("newsletter", _options.EnableNewsletter),
            Visuals = Cap("visuals", _options.EnableVisuals),
            StoreContextHeaders = Cap("storeContextHeaders", _options.EnableStoreContextHeaders)
        };
    }

    private StorefrontEditableConfigDto BuildDefaultEditableConfig(string key)
    {
        return new StorefrontEditableConfigDto
        {
            StorefrontKey = key,
            StoreName = _options.StoreName,
            Tagline = _options.Tagline,
            LogoUrl = _options.LogoUrl,
            FaviconUrl = _options.FaviconUrl,
            SupportEmail = _options.SupportEmail,
            SupportPhone = _options.SupportPhone,
            TimeZone = NormalizeString(_options.TimeZone, "UTC"),
            DefaultLocale = _options.DefaultLocale,
            Currency = _options.Currency,
            Country = _options.Country,
            ThemePreset = _options.ThemePreset,
            LayoutPreset = _options.LayoutPreset,
            ProductCardStyle = _options.ProductCardStyle,
            ColorTokens = CopyDictionary(_options.ThemeColors),
            TypographyTokens = CopyDictionary(_options.TypographyTokens),
            SpacingTokens = CopyDictionary(_options.SpacingTokens),
            HomepageSections = BuildDefaultSections(),
            CapabilityOverrides = new Dictionary<string, bool>(),
            Shipping = new StorefrontShippingConfigDto
            {
                Strategy = NormalizeString(_options.Shipping.Strategy, "threshold"),
                FlatRate = _options.Shipping.FlatRate,
                FreeShippingThreshold = _options.Shipping.FreeShippingThreshold,
                PerKgRate = _options.Shipping.PerKgRate,
                HandlingFee = _options.Shipping.HandlingFee,
                Currency = NormalizeString(_options.Shipping.Currency, _options.Currency),
                Label = NormalizeString(_options.Shipping.Label, "Standard Shipping")
            },
            Tax = new StorefrontTaxConfigDto
            {
                Strategy = NormalizeString(_options.Tax.Strategy, "percentage"),
                RatePercent = _options.Tax.RatePercent,
                PricesIncludeTax = _options.Tax.PricesIncludeTax,
                AppliesToShipping = _options.Tax.AppliesToShipping,
                Label = NormalizeString(_options.Tax.Label, "GST")
            },
            SeoTitle = _options.DefaultSeoTitle,
            SeoDescription = _options.DefaultSeoDescription,
            SeoKeywords = NormalizeList(_options.SeoKeywords),
            UpdatedAtUtc = default,
            UpdatedBy = string.Empty
        };
    }

    private List<StorefrontSectionDto> BuildDefaultSections()
    {
        var sections = new List<StorefrontSectionDto>();
        var index = 0;
        foreach (var raw in NormalizeList(_options.HomepageSections))
        {
            sections.Add(new StorefrontSectionDto
            {
                Id = raw,
                Type = raw,
                Title = Humanize(raw),
                Subtitle = string.Empty,
                Enabled = true,
                SortOrder = (index + 1) * 10,
                Settings = new Dictionary<string, string>()
            });
            index++;
        }

        return sections;
    }

    private StorefrontEditableConfigDto MergeEditable(StorefrontEditableConfigDto baseCfg, StorefrontEditableConfigDto ovr, string key)
    {
        return new StorefrontEditableConfigDto
        {
            StorefrontKey = key,
            StoreName = Coalesce(ovr.StoreName, baseCfg.StoreName),
            Tagline = Coalesce(ovr.Tagline, baseCfg.Tagline),
            LogoUrl = Coalesce(ovr.LogoUrl, baseCfg.LogoUrl),
            FaviconUrl = Coalesce(ovr.FaviconUrl, baseCfg.FaviconUrl),
            SupportEmail = Coalesce(ovr.SupportEmail, baseCfg.SupportEmail),
            SupportPhone = Coalesce(ovr.SupportPhone, baseCfg.SupportPhone),
            TimeZone = Coalesce(ovr.TimeZone, baseCfg.TimeZone),
            DefaultLocale = Coalesce(ovr.DefaultLocale, baseCfg.DefaultLocale),
            Currency = Coalesce(ovr.Currency, baseCfg.Currency),
            Country = Coalesce(ovr.Country, baseCfg.Country),
            ThemePreset = Coalesce(ovr.ThemePreset, baseCfg.ThemePreset),
            LayoutPreset = Coalesce(ovr.LayoutPreset, baseCfg.LayoutPreset),
            ProductCardStyle = Coalesce(ovr.ProductCardStyle, baseCfg.ProductCardStyle),
            ColorTokens = MergeDictionary(baseCfg.ColorTokens, ovr.ColorTokens),
            TypographyTokens = MergeDictionary(baseCfg.TypographyTokens, ovr.TypographyTokens),
            SpacingTokens = MergeDictionary(baseCfg.SpacingTokens, ovr.SpacingTokens),
            HomepageSections = ovr.HomepageSections != null && ovr.HomepageSections.Count > 0 ? ovr.HomepageSections : baseCfg.HomepageSections,
            CapabilityOverrides = ovr.CapabilityOverrides ?? new Dictionary<string, bool>(),
            Shipping = ovr.Shipping != null && !string.IsNullOrWhiteSpace(ovr.Shipping.Strategy) ? ovr.Shipping : baseCfg.Shipping,
            Tax = ovr.Tax != null && !string.IsNullOrWhiteSpace(ovr.Tax.Strategy) ? ovr.Tax : baseCfg.Tax,
            SeoTitle = Coalesce(ovr.SeoTitle, baseCfg.SeoTitle),
            SeoDescription = Coalesce(ovr.SeoDescription, baseCfg.SeoDescription),
            SeoKeywords = ovr.SeoKeywords != null && ovr.SeoKeywords.Count > 0 ? NormalizeList(ovr.SeoKeywords) : baseCfg.SeoKeywords,
            UpdatedAtUtc = ovr.UpdatedAtUtc,
            UpdatedBy = ovr.UpdatedBy
        };
    }

    private static StorefrontShippingConfigDto CloneShipping(StorefrontShippingConfigDto source) => new()
    {
        Strategy = source.Strategy,
        FlatRate = source.FlatRate,
        FreeShippingThreshold = source.FreeShippingThreshold,
        PerKgRate = source.PerKgRate,
        HandlingFee = source.HandlingFee,
        Currency = source.Currency,
        Label = source.Label
    };

    private static StorefrontTaxConfigDto CloneTax(StorefrontTaxConfigDto source) => new()
    {
        Strategy = source.Strategy,
        RatePercent = source.RatePercent,
        PricesIncludeTax = source.PricesIncludeTax,
        AppliesToShipping = source.AppliesToShipping,
        Label = source.Label
    };

    private static List<StorefrontEndpointDto> BuildEndpoints(string apiBasePath)
    {
        return new List<StorefrontEndpointDto>
        {
            Endpoint("storefront.config", "GET", $"{apiBasePath}/storefront/config", false, "Load storefront bootstrap contract"),
            Endpoint("storefront.theme", "GET", $"{apiBasePath}/storefront/theme", false, "Load storefront theme tokens"),
            Endpoint("storefront.home", "GET", $"{apiBasePath}/storefront/pages/home", false, "Load homepage layout sections"),
            Endpoint("storefront.stores", "GET", $"{apiBasePath}/storefront/stores", false, "List storefront tenants"),
            Endpoint("storefront.shippingQuote", "POST", $"{apiBasePath}/storefront/shipping/quote", false, "Compute a shipping quote"),
            Endpoint("storefront.taxQuote", "POST", $"{apiBasePath}/storefront/tax/quote", false, "Compute a tax quote"),
            Endpoint("auth.register", "POST", $"{apiBasePath}/auth/register", false, "Create a customer account"),
            Endpoint("auth.login", "POST", $"{apiBasePath}/auth/login", false, "Login and receive tokens"),
            Endpoint("auth.refresh", "POST", $"{apiBasePath}/auth/refresh-token", false, "Refresh access token"),
            Endpoint("auth.profile", "GET", $"{apiBasePath}/auth/profile", true, "Load authenticated user profile"),
            Endpoint("products.search", "GET", $"{apiBasePath}/products", false, "Browse and filter products"),
            Endpoint("products.featured", "GET", $"{apiBasePath}/products/featured", false, "Load featured products"),
            Endpoint("products.byId", "GET", $"{apiBasePath}/products/{{id}}", false, "Load product details"),
            Endpoint("products.bySku", "GET", $"{apiBasePath}/products/sku/{{sku}}", false, "Load product by SKU"),
            Endpoint("categories.list", "GET", $"{apiBasePath}/categories", false, "Load categories"),
            Endpoint("categories.tree", "GET", $"{apiBasePath}/categories/tree", false, "Load nested category navigation"),
            Endpoint("brands.list", "GET", $"{apiBasePath}/brands", false, "Load brands"),
            Endpoint("offers.active", "GET", $"{apiBasePath}/offers/active", false, "Load active merchandising offers"),
            Endpoint("discounts.active", "GET", $"{apiBasePath}/discounts/active", false, "Load active discounts"),
            Endpoint("vouchers.validate", "POST", $"{apiBasePath}/vouchers/validate", false, "Validate voucher before checkout"),
            Endpoint("cart.current", "GET", $"{apiBasePath}/cart", true, "Load current cart"),
            Endpoint("cart.add", "POST", $"{apiBasePath}/cart/items", true, "Add item to cart"),
            Endpoint("orders.mine", "GET", $"{apiBasePath}/orders/my", true, "Load customer orders"),
            Endpoint("orders.create", "POST", $"{apiBasePath}/orders", true, "Create order from cart"),
            Endpoint("payments.create", "POST", $"{apiBasePath}/payments", true, "Create payment"),
            Endpoint("wishlist.current", "GET", $"{apiBasePath}/wishlist", true, "Load wishlist"),
            Endpoint("reviews.product", "GET", $"{apiBasePath}/reviews/product/{{productId}}", false, "Load product reviews"),
            Endpoint("reviews.create", "POST", $"{apiBasePath}/reviews", true, "Create product review"),
            Endpoint("newsletter.subscribe", "POST", $"{apiBasePath}/newsletter/subscribe", false, "Subscribe to newsletter"),
            Endpoint("blog.list", "GET", $"{apiBasePath}/blog", false, "Load blog posts"),
            Endpoint("contact.create", "POST", $"{apiBasePath}/contact", false, "Send contact message"),
            Endpoint("waitlist.subscribe", "POST", $"{apiBasePath}/waitlist/subscribe", false, "Join waitlist"),
            Endpoint("subscriptions.current", "GET", $"{apiBasePath}/subscription/current", true, "Load current subscription"),
            Endpoint("visuals.list", "GET", $"{apiBasePath}/visuals", false, "Load digital visual products")
        };
    }

    private static StorefrontEndpointDto Endpoint(string id, string method, string path, bool requiresAuth, string useCase)
    {
        return new StorefrontEndpointDto
        {
            Id = id,
            Method = method,
            Path = path,
            RequiresAuth = requiresAuth,
            UseCase = useCase
        };
    }

    private static string NormalizeApiBasePath(string? apiBasePath)
    {
        if (string.IsNullOrWhiteSpace(apiBasePath))
            return "/api";

        var trimmed = apiBasePath.Trim().TrimEnd('/');
        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }

    private List<string> BuildSupportedLocales(string defaultLocale)
    {
        return NormalizeList(new[] { defaultLocale }.Concat(_options.SupportedLocales));
    }

    private static List<StorefrontLinkDto> BuildLinks(IEnumerable<StorefrontLinkOptions>? links)
    {
        return (links ?? Enumerable.Empty<StorefrontLinkOptions>())
            .Where(link => !string.IsNullOrWhiteSpace(link.Id) && !string.IsNullOrWhiteSpace(link.Label) && !string.IsNullOrWhiteSpace(link.Url))
            .Select(link => new StorefrontLinkDto
            {
                Id = link.Id.Trim(),
                Label = link.Label.Trim(),
                Url = link.Url.Trim(),
                Icon = link.Icon.Trim(),
                OpenInNewTab = link.OpenInNewTab
            })
            .GroupBy(link => link.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static List<StorefrontNavigationLinkDto> BuildNavigationLinks(IEnumerable<StorefrontNavigationLinkOptions>? links)
    {
        return (links ?? Enumerable.Empty<StorefrontNavigationLinkOptions>())
            .Where(link => !string.IsNullOrWhiteSpace(link.Id) && !string.IsNullOrWhiteSpace(link.Label) && !string.IsNullOrWhiteSpace(link.Path) && !string.IsNullOrWhiteSpace(link.Slot))
            .Select(link => new StorefrontNavigationLinkDto
            {
                Id = link.Id.Trim(),
                Label = link.Label.Trim(),
                Path = link.Path.Trim(),
                Slot = link.Slot.Trim(),
                SortOrder = link.SortOrder,
                RequiresAuth = link.RequiresAuth,
                RequiredRole = link.RequiredRole.Trim()
            })
            .GroupBy(link => link.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(link => link.Slot, StringComparer.OrdinalIgnoreCase)
            .ThenBy(link => link.SortOrder)
            .ThenBy(link => link.Label, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> NormalizeList(IEnumerable<string>? values)
    {
        return (values ?? Enumerable.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Dictionary<string, string> CopyDictionary(IDictionary<string, string>? values)
    {
        return (values ?? new Dictionary<string, string>())
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .ToDictionary(pair => pair.Key.Trim(), pair => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> MergeDictionary(IDictionary<string, string>? baseValues, IDictionary<string, string>? overrideValues)
    {
        var result = CopyDictionary(baseValues);
        foreach (var pair in CopyDictionary(overrideValues))
            result[pair.Key] = pair.Value;
        return result;
    }

    private static string Coalesce(string? preferred, string fallback)
    {
        return string.IsNullOrWhiteSpace(preferred) ? fallback : preferred.Trim();
    }

    private static string NormalizeString(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var spaced = System.Text.RegularExpressions.Regex.Replace(value.Trim(), "(?<=[a-z0-9])(?=[A-Z])", " ");
        spaced = spaced.Replace('-', ' ').Replace('_', ' ');
        return char.ToUpperInvariant(spaced[0]) + spaced[1..];
    }
}
