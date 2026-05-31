using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AppilicoShopServer.Domain.Constants;
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

    [Fact]
    public async Task GetHomePage_WithoutAuth_ReturnsEnabledSections()
    {
        var response = await _client.GetAsync("/api/storefront/pages/home");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        data.GetProperty("sections").EnumerateArray().Should().NotBeEmpty();
        data.GetProperty("sections").EnumerateArray().Should().OnlyContain(section => section.GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public async Task GetStores_WithoutAuth_AlwaysIncludesDefault()
    {
        var response = await _client.GetAsync("/api/storefront/stores");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        data.EnumerateArray().Should().Contain(store => store.GetProperty("isDefault").GetBoolean());
    }

    [Fact]
    public async Task PostShippingQuote_ReturnsComputedShippingCost()
    {
        var response = await _client.PostAsJsonAsync("/api/storefront/shipping/quote", new
        {
            subtotal = 50m,
            itemCount = 2,
            totalWeightKg = 1m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("total").GetDecimal().Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task PostTaxQuote_ReturnsComputedTax()
    {
        var response = await _client.PostAsJsonAsync("/api/storefront/tax/quote", new
        {
            subtotal = 100m,
            shippingAmount = 10m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("totalWithTax").GetDecimal().Should().BeGreaterThanOrEqualTo(100m);
    }

    [Fact]
    public async Task AdminGetConfig_WithoutAuth_IsUnauthorized()
    {
        var response = await _client.GetAsync("/api/storefront/admin/config");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUpdateConfig_AsAdmin_PersistsAndReturnsUpdatedStoreName()
    {
        var newName = $"Engine Store {Guid.NewGuid():N}";

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/storefront/admin/config")
        {
            Content = JsonContent.Create(new { storeName = newName })
        };
        request.Headers.Add(TestAuthHandler.UserIdHeader, "admin-engine");
        request.Headers.Add(TestAuthHandler.RoleHeader, AppConstants.Roles.Admin);

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("data").GetProperty("storeName").GetString().Should().Be(newName);
    }

    [Fact]
    public async Task AdminUpsertAndDeleteStore_AsAdmin_Succeeds()
    {
        var key = $"tenant-{Guid.NewGuid():N}".Substring(0, 16);

        using var upsert = new HttpRequestMessage(HttpMethod.Post, "/api/storefront/admin/stores")
        {
            Content = JsonContent.Create(new { storefrontKey = key, storeName = "Integration Tenant" })
        };
        upsert.Headers.Add(TestAuthHandler.UserIdHeader, "admin-engine");
        upsert.Headers.Add(TestAuthHandler.RoleHeader, AppConstants.Roles.Admin);

        var upsertResponse = await _client.SendAsync(upsert);
        upsertResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/storefront/admin/stores/{key}");
        deleteRequest.Headers.Add(TestAuthHandler.UserIdHeader, "admin-engine");
        deleteRequest.Headers.Add(TestAuthHandler.RoleHeader, AppConstants.Roles.Admin);

        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
