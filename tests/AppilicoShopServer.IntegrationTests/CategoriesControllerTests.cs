using System.Net;
using FluentAssertions;

namespace AppilicoShopServer.IntegrationTests;

public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryTree_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/categories/tree");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategoryById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
