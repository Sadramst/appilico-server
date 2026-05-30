using System.Net;
using FluentAssertions;

namespace AppilicoShopServer.IntegrationTests;

public class CartControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CartControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCart_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyOrders_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/orders/my");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class WishlistControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WishlistControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/wishlist");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class SettingsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SettingsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSettings_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/settings");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
