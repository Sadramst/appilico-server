using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Appilico.Server.IntegrationTests;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFeaturedProducts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products/featured");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductBySku_WithInvalidSku_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/sku/INVALID-SKU-XYZ");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ReturnsUnauthorized()
    {
        var product = new { Name = "Test", Description = "Test", SKU = "TEST-001", BasePrice = 9.99, CategoryId = Guid.NewGuid(), BrandId = Guid.NewGuid() };

        var response = await _client.PostAsJsonAsync("/api/products", product);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
