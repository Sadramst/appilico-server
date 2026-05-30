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
        result.Data.Brand.StoreName.Should().Be("Appilico Shop");
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
    public async Task GetConfigAsync_CustomOptions_OverridesStorefrontMetadata()
    {
        var service = CreateService(new StorefrontOptions
        {
            StoreName = "Reusable Demo Shop",
            ApiBasePath = "api/v2",
            PublicBaseUrl = "https://shop.example.test",
            Currency = "EUR",
            Country = "DE",
            DefaultLocale = "de-DE",
            SupportedLocales = new List<string> { "de-DE", "en-GB" },
            ThemePreset = "quiet-luxury",
            EnableWishlist = false,
            EnableReviews = false
        });

        var result = await service.GetConfigAsync();

        result.Data!.Brand.StoreName.Should().Be("Reusable Demo Shop");
        result.Data.Brand.PublicBaseUrl.Should().Be("https://shop.example.test");
        result.Data.Locale.Currency.Should().Be("EUR");
        result.Data.Locale.Country.Should().Be("DE");
        result.Data.Locale.DefaultLocale.Should().Be("de-DE");
        result.Data.Locale.SupportedLocales.Should().Equal("de-DE", "en-GB");
        result.Data.Theme.Preset.Should().Be("quiet-luxury");
        result.Data.Capabilities.Wishlist.Should().BeFalse();
        result.Data.Capabilities.Reviews.Should().BeFalse();
        result.Data.Endpoints.Should().Contain(endpoint => endpoint.Path == "/api/v2/products");
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
