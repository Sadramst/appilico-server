using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace AppilicoShopServer.IntegrationTests;

public class StorefrontControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StorefrontControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetConfig_WithoutAuth_ReturnsPublicEngineContract()
    {
        var response = await _client.GetAsync("/api/storefront/config");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = root.GetProperty("data");
        data.GetProperty("engineName").GetString().Should().Be("AppilicoShopServer");
        data.GetProperty("storefrontKey").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("brand").GetProperty("storeName").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("brand").GetProperty("legalLinks").EnumerateArray().Should().NotBeEmpty();
        data.GetProperty("capabilities").GetProperty("productCatalog").GetBoolean().Should().BeTrue();
        data.GetProperty("context").GetProperty("headerName").GetString().Should().Be("X-Storefront-Key");
        data.GetProperty("theme").GetProperty("colorTokens").GetProperty("primary").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("endpoints").EnumerateArray()
            .Should()
            .Contain(endpoint => endpoint.GetProperty("id").GetString() == "products.search");
    }

    [Fact]
    public async Task GetTheme_WithoutAuth_ReturnsPublicThemeContract()
    {
        var response = await _client.GetAsync("/api/storefront/theme");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        root.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = root.GetProperty("data");
        data.GetProperty("preset").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("layoutPreset").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("colorTokens").GetProperty("primary").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("homepageSections").EnumerateArray().Should().Contain(section => section.GetString() == "featuredProducts");
    }
}
