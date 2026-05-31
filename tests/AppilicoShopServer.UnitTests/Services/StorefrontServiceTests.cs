using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Constants;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AppilicoShopServer.UnitTests.Services;

public class StorefrontServiceTests
{
    [Fact]
    public async Task GetConfigAsync_DefaultOptions_ReturnsReusableShopEngineContract()
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.EngineName.Should().Be("AppilicoShopServer");
        result.Data.StorefrontKey.Should().Be("default");
        result.Data.StorefrontMode.Should().Be("single-store");
        result.Data.Brand.StoreName.Should().Be("Appilico Shop");
        result.Data.Brand.Tagline.Should().NotBeNullOrWhiteSpace();
        result.Data.Brand.SupportEmail.Should().Be("support@appilico.com.au");
        result.Data.Brand.LegalLinks.Should().Contain(link => link.Id == "privacy" && link.Url == "/privacy");
        result.Data.Context.HeaderName.Should().Be("X-Storefront-Key");
        result.Data.Seo.DefaultTitle.Should().Be("Appilico Shop");
        result.Data.ApiVersion.Should().Be("v1");
        result.Data.GeneratedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetConfigAsync_DefaultOptions_EnablesCoreCommerceCapabilities()
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        var capabilities = result.Data!.Capabilities;
        capabilities.ProductCatalog.Should().BeTrue();
        capabilities.CategoryTree.Should().BeTrue();
        capabilities.Brands.Should().BeTrue();
        capabilities.Cart.Should().BeTrue();
        capabilities.CustomerAccounts.Should().BeTrue();
        capabilities.Orders.Should().BeTrue();
        capabilities.Payments.Should().BeTrue();
        capabilities.Discounts.Should().BeTrue();
        capabilities.Vouchers.Should().BeTrue();
        capabilities.Wishlist.Should().BeTrue();
        capabilities.Reviews.Should().BeTrue();
        capabilities.StoreContextHeaders.Should().BeTrue();
    }

    [Fact]
    public async Task GetConfigAsync_DefaultOptions_ReturnsClientSafeThemeAndNavigation()
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        result.Data!.Theme.ColorTokens.Should().ContainKey("primary");
        result.Data.Theme.TypographyTokens.Should().ContainKey("headingFont");
        result.Data.Theme.SpacingTokens.Should().ContainKey("cardRadius");
        result.Data.Theme.HomepageSections.Should().ContainInOrder("hero", "featuredProducts", "categoryRail");
        result.Data.Locale.SupportedLocales.Should().Equal("en-AU");
        result.Data.Navigation.Links.Should().Contain(link => link.Id == "catalog" && link.Path == "/products");
        result.Data.Navigation.Links.Should().Contain(link => link.Id == "account" && link.RequiresAuth);
    }

    [Fact]
    public async Task GetConfigAsync_CustomOptions_OverridesStorefrontMetadata()
    {
        var service = CreateService(new StorefrontOptions
        {
            StorefrontKey = "demo-shop",
            StoreName = "Reusable Demo Shop",
            Tagline = "Reusable demo tagline",
            LogoUrl = "https://cdn.example.test/logo.svg",
            FaviconUrl = "https://cdn.example.test/favicon.ico",
            SupportEmail = "support@example.test",
            SupportPhone = "+61 400 000 000",
            TimeZone = "Europe/Berlin",
            ApiBasePath = "api/v2",
            PublicBaseUrl = "https://shop.example.test",
            Currency = "EUR",
            Country = "DE",
            DefaultLocale = "de-DE",
            SupportedLocales = new List<string> { "de-DE", "en-GB", "de-DE" },
            ThemePreset = "quiet-luxury",
            LayoutPreset = "editorial-commerce",
            ProductCardStyle = "compact",
            ThemeColors = new Dictionary<string, string>
            {
                ["primary"] = "#101010",
                ["accent"] = "#f97316"
            },
            HomepageSections = new List<string> { "hero", "featuredProducts", "hero" },
            NavigationLinks = new List<StorefrontNavigationLinkOptions>
            {
                new() { Id = "shop", Label = "Shop", Path = "/shop", Slot = "primary", SortOrder = 10 },
                new() { Id = "vip", Label = "VIP", Path = "/vip", Slot = "account", SortOrder = 20, RequiresAuth = true }
            },
            SocialLinks = new List<StorefrontLinkOptions>
            {
                new() { Id = "instagram", Label = "Instagram", Url = "https://instagram.example.test", Icon = "instagram", OpenInNewTab = true }
            },
            LegalLinks = new List<StorefrontLinkOptions>
            {
                new() { Id = "returns", Label = "Returns", Url = "/policies/returns" }
            },
            StoreContextHeaderName = "X-Shop-Key",
            StoreContextResolutionStrategy = "host-or-header",
            SupportsMultiStore = true,
            DefaultSeoTitle = "Reusable Demo Shop",
            DefaultSeoDescription = "Demo SEO description",
            SeoKeywords = new List<string> { "demo", "shop" },
            ShippingStrategy = "flat-rate",
            TaxStrategy = "vat-inclusive",
            ReturnsPolicyUrl = "/policies/returns",
            TermsUrl = "/policies/terms",
            PrivacyUrl = "/policies/privacy",
            EnableWishlist = false,
            EnableReviews = false
        });

        var result = await service.GetConfigAsync();

        result.Data!.StorefrontKey.Should().Be("demo-shop");
        result.Data!.Brand.StoreName.Should().Be("Reusable Demo Shop");
        result.Data.Brand.Tagline.Should().Be("Reusable demo tagline");
        result.Data.Brand.LogoUrl.Should().Be("https://cdn.example.test/logo.svg");
        result.Data.Brand.FaviconUrl.Should().Be("https://cdn.example.test/favicon.ico");
        result.Data.Brand.PublicBaseUrl.Should().Be("https://shop.example.test");
        result.Data.Brand.SupportEmail.Should().Be("support@example.test");
        result.Data.Brand.SupportPhone.Should().Be("+61 400 000 000");
        result.Data.Brand.TimeZone.Should().Be("Europe/Berlin");
        result.Data.Brand.SocialLinks.Should().ContainSingle(link => link.Id == "instagram" && link.OpenInNewTab);
        result.Data.Brand.LegalLinks.Should().ContainSingle(link => link.Url == "/policies/returns");
        result.Data.Locale.Currency.Should().Be("EUR");
        result.Data.Locale.Country.Should().Be("DE");
        result.Data.Locale.DefaultLocale.Should().Be("de-DE");
        result.Data.Locale.SupportedLocales.Should().Equal("de-DE", "en-GB");
        result.Data.Theme.Preset.Should().Be("quiet-luxury");
        result.Data.Theme.LayoutPreset.Should().Be("editorial-commerce");
        result.Data.Theme.ProductCardStyle.Should().Be("compact");
        result.Data.Theme.ColorTokens.Should().Contain("primary", "#101010");
        result.Data.Theme.HomepageSections.Should().Equal("hero", "featuredProducts");
        result.Data.Navigation.Links.Should().Contain(link => link.Id == "vip" && link.RequiresAuth);
        result.Data.Context.HeaderName.Should().Be("X-Shop-Key");
        result.Data.Context.ResolutionStrategy.Should().Be("host-or-header");
        result.Data.Context.SupportsMultiStore.Should().BeTrue();
        result.Data.Seo.Keywords.Should().Equal("demo", "shop");
        result.Data.Checkout.ShippingStrategy.Should().Be("flat-rate");
        result.Data.Checkout.TaxStrategy.Should().Be("vat-inclusive");
        result.Data.Checkout.ReturnsPolicyUrl.Should().Be("/policies/returns");
        result.Data.Capabilities.Wishlist.Should().BeFalse();
        result.Data.Capabilities.Reviews.Should().BeFalse();
        result.Data.Endpoints.Should().Contain(endpoint => endpoint.Path == "/api/v2/products");
    }

    [Fact]
    public async Task GetThemeAsync_ReturnsStandaloneThemeContract()
    {
        var service = CreateService(new StorefrontOptions
        {
            ThemePreset = "brand-a",
            LayoutPreset = "dense-catalog",
            ProductCardStyle = "quick-buy",
            ThemeColors = new Dictionary<string, string>
            {
                ["primary"] = "#0f766e"
            }
        });

        var result = await service.GetThemeAsync();

        result.Success.Should().BeTrue();
        result.Data!.Preset.Should().Be("brand-a");
        result.Data.LayoutPreset.Should().Be("dense-catalog");
        result.Data.ProductCardStyle.Should().Be("quick-buy");
        result.Data.ColorTokens.Should().Contain("primary", "#0f766e");
        result.Data.ProductCardFields.Should().Contain("basePrice");
    }

    [Fact]
    public async Task GetConfigAsync_AuthContract_ExposesCustomerAndPrivilegedRoles()
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        result.Data!.Auth.TokenScheme.Should().Be("Bearer");
        result.Data.Auth.CustomerRole.Should().Be(AppConstants.Roles.Customer);
        result.Data.Auth.PrivilegedRoles.Should().Equal(
            AppConstants.Roles.SuperAdmin,
            AppConstants.Roles.Admin,
            AppConstants.Roles.Manager);
    }

    [Fact]
    public async Task GetConfigAsync_NavigationAndCheckout_ReferenceStableEndpointIds()
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        var endpointIds = result.Data!.Endpoints.Select(endpoint => endpoint.Id).ToHashSet();
        endpointIds.Should().Contain(result.Data.Navigation.CategoryTreeEndpointId);
        endpointIds.Should().Contain(result.Data.Checkout.CartEndpointId);
        endpointIds.Should().Contain(result.Data.Checkout.CreateOrderEndpointId);
        endpointIds.Should().Contain(result.Data.Checkout.CreatePaymentEndpointId);
        result.Data.Navigation.Slots.Should().Contain(new[] { "primary", "mobile", "footer", "account" });
        result.Data.Navigation.Links.Select(link => link.Id).Should().Contain(new[] { "home", "catalog", "account" });
    }

    [Theory]
    [MemberData(nameof(ExpectedEndpointContracts))]
    public async Task GetConfigAsync_ExposesExpectedClientEndpoint(string id, string method, string path, bool requiresAuth)
    {
        var service = CreateService();

        var result = await service.GetConfigAsync();

        var endpoint = result.Data!.Endpoints.Should().ContainSingle(item => item.Id == id).Subject;
        endpoint.Method.Should().Be(method);
        endpoint.Path.Should().Be(path);
        endpoint.RequiresAuth.Should().Be(requiresAuth);
        endpoint.UseCase.Should().NotBeNullOrWhiteSpace();
    }

    public static IEnumerable<object[]> ExpectedEndpointContracts()
    {
        yield return Endpoint("storefront.config", "GET", "/api/storefront/config", false);
        yield return Endpoint("storefront.theme", "GET", "/api/storefront/theme", false);
        yield return Endpoint("auth.register", "POST", "/api/auth/register", false);
        yield return Endpoint("auth.login", "POST", "/api/auth/login", false);
        yield return Endpoint("auth.refresh", "POST", "/api/auth/refresh-token", false);
        yield return Endpoint("auth.profile", "GET", "/api/auth/profile", true);
        yield return Endpoint("products.search", "GET", "/api/products", false);
        yield return Endpoint("products.featured", "GET", "/api/products/featured", false);
        yield return Endpoint("products.byId", "GET", "/api/products/{id}", false);
        yield return Endpoint("products.bySku", "GET", "/api/products/sku/{sku}", false);
        yield return Endpoint("categories.list", "GET", "/api/categories", false);
        yield return Endpoint("categories.tree", "GET", "/api/categories/tree", false);
        yield return Endpoint("brands.list", "GET", "/api/brands", false);
        yield return Endpoint("offers.active", "GET", "/api/offers/active", false);
        yield return Endpoint("discounts.active", "GET", "/api/discounts/active", false);
        yield return Endpoint("vouchers.validate", "POST", "/api/vouchers/validate", false);
        yield return Endpoint("cart.current", "GET", "/api/cart", true);
        yield return Endpoint("cart.add", "POST", "/api/cart/items", true);
        yield return Endpoint("orders.mine", "GET", "/api/orders/my", true);
        yield return Endpoint("orders.create", "POST", "/api/orders", true);
        yield return Endpoint("payments.create", "POST", "/api/payments", true);
        yield return Endpoint("wishlist.current", "GET", "/api/wishlist", true);
        yield return Endpoint("reviews.product", "GET", "/api/reviews/product/{productId}", false);
        yield return Endpoint("reviews.create", "POST", "/api/reviews", true);
        yield return Endpoint("newsletter.subscribe", "POST", "/api/newsletter/subscribe", false);
        yield return Endpoint("blog.list", "GET", "/api/blog", false);
        yield return Endpoint("contact.create", "POST", "/api/contact", false);
        yield return Endpoint("waitlist.subscribe", "POST", "/api/waitlist/subscribe", false);
        yield return Endpoint("subscriptions.current", "GET", "/api/subscription/current", true);
        yield return Endpoint("visuals.list", "GET", "/api/visuals", false);
    }

    private static StorefrontService CreateService(StorefrontOptions? options = null)
    {
        return new StorefrontService(Microsoft.Extensions.Options.Options.Create(options ?? new StorefrontOptions()));
    }

    private static object[] Endpoint(string id, string method, string path, bool requiresAuth)
    {
        return new object[] { id, method, path, requiresAuth };
    }
}
