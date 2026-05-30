using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace AppilicoShopServer.IntegrationTests;

public class BrandsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BrandsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBrands_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/brands");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBrandById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/brands/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBrand_WithoutAuth_ReturnsUnauthorized()
    {
        var brand = new { Name = "TestBrand", Description = "Test" };

        var response = await _client.PostAsJsonAsync("/api/brands", brand);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
