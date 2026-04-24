using System.Net;
using FluentAssertions;

namespace Appilico.Server.IntegrationTests;

public class DiscountsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiscountsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActiveDiscounts_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/discounts/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDiscounts_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/discounts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class VouchersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VouchersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetVouchers_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/vouchers");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class OffersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OffersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActiveOffers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/offers/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOffers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/offers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
