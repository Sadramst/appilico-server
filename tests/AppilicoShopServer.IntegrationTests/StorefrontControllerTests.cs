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
        data.GetProperty("brand").GetProperty("storeName").GetString().Should().NotBeNullOrWhiteSpace();
        data.GetProperty("capabilities").GetProperty("productCatalog").GetBoolean().Should().BeTrue();
        data.GetProperty("endpoints").EnumerateArray()
            .Should()
            .Contain(endpoint => endpoint.GetProperty("id").GetString() == "products.search");
    }
}
