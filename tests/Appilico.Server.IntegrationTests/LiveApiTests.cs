using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace Appilico.Server.IntegrationTests;

/// <summary>
/// Shared fixture that logs in once and caches tokens for all tests.
/// </summary>
public class LiveApiFixture : IAsyncLifetime
{
    public string BaseUrl { get; }
    public string AdminToken { get; private set; } = "";
    public string CustomerToken { get; private set; } = "";
    public string AdminRefreshToken { get; private set; } = "";

    public LiveApiFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("PRIMO_API_BASE_URL")
                  ?? "http://localhost:5034";
    }

    public async Task InitializeAsync()
    {
        using var client = new HttpClient { BaseAddress = new Uri(BaseUrl), Timeout = TimeSpan.FromSeconds(30) };

        var adminLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@appilico.com", password = "Admin@123!" });
        adminLogin.StatusCode.Should().Be(HttpStatusCode.OK, "Admin login should succeed");
        var adminData = JsonDocument.Parse(await adminLogin.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data");
        AdminToken = adminData.GetProperty("accessToken").GetString()!;
        AdminRefreshToken = adminData.GetProperty("refreshToken").GetString()!;

        var custLogin = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "customer1@appilico.com", password = "Customer@123!" });
        custLogin.StatusCode.Should().Be(HttpStatusCode.OK, "Customer login should succeed");
        var custData = JsonDocument.Parse(await custLogin.Content.ReadAsStringAsync())
            .RootElement.GetProperty("data");
        CustomerToken = custData.GetProperty("accessToken").GetString()!;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("LiveApi")]
public class LiveApiCollection : ICollectionFixture<LiveApiFixture> { }

/// <summary>
/// Live API integration tests against a real running server.
/// Set PRIMO_API_BASE_URL env var to target a specific server.
/// Defaults to http://localhost:5034 (local dev).
/// Use https://appilico-server.onrender.com for deployed.
/// </summary>
[Collection("LiveApi")]
public class LiveApiTests
{
    private readonly HttpClient _client;
    private readonly string _adminToken;
    private readonly string _customerToken;
    private readonly string _adminRefreshToken;

    public LiveApiTests(LiveApiFixture fixture)
    {
        _client = new HttpClient { BaseAddress = new Uri(fixture.BaseUrl), Timeout = TimeSpan.FromSeconds(30) };
        _adminToken = fixture.AdminToken;
        _customerToken = fixture.CustomerToken;
        _adminRefreshToken = fixture.AdminRefreshToken;
    }

    // ── Helpers ──

    private async Task<HttpResponseMessage> GetAuth(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private async Task<HttpResponseMessage> PostAuth(string url, object body, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private async Task<HttpResponseMessage> PostAuthEmpty(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private async Task<HttpResponseMessage> PutAuth(string url, object body, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private async Task<HttpResponseMessage> DeleteAuth(string url, string token)
    {
        var req = new HttpRequestMessage(HttpMethod.Delete, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(req);
    }

    private static async Task<JsonElement> Root(HttpResponseMessage resp)
    {
        var body = await resp.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task<JsonElement> Data(HttpResponseMessage resp)
    {
        return (await Root(resp)).GetProperty("data");
    }

    // ═══════════════════════════════════════════════════
    //  SWAGGER
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Swagger_ReturnsOk()
    {
        var resp = await _client.GetAsync("/swagger/index.html");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ═══════════════════════════════════════════════════
    //  AUTH  (8 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Auth_Login_Admin_ReturnsTokenAndRoles()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@appilico.com", password = "Admin@123!" });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("user").GetProperty("roles").EnumerateArray()
            .Select(r => r.GetString()).Should().Contain("Admin");
    }

    [Fact]
    public async Task Auth_Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "admin@appilico.com", password = "WrongPassword" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Auth_AdminToken_IsValid()
    {
        _adminToken.Should().NotBeNullOrEmpty("Admin token should be set from fixture login");
    }

    [Fact]
    public void Auth_CustomerToken_IsValid()
    {
        _customerToken.Should().NotBeNullOrEmpty("Customer token should be set from fixture login");
    }

    [Fact]
    public async Task Auth_GetProfile_WithToken_ReturnsProfile()
    {
        var resp = await GetAuth("/api/auth/profile", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("email").GetString().Should().Be("admin@appilico.com");
    }

    [Fact]
    public async Task Auth_GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/auth/profile");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Auth_Register_DuplicateEmail_ReturnsBadRequest()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test", lastName = "User",
            email = "admin@appilico.com",
            password = "Password123!", confirmPassword = "Password123!"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ═══════════════════════════════════════════════════
    //  PRODUCTS  (14 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Products_Search_ReturnsPagedResults()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=5");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = await Root(resp);
        root.GetProperty("success").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
        root.GetProperty("pagination").GetProperty("totalCount").GetInt32().Should().Be(74);
    }

    [Fact]
    public async Task Products_Search_WithSearchTerm_FiltersResults()
    {
        var resp = await _client.GetAsync("/api/products?searchTerm=scotch&page=1&pageSize=10");
        var data = await Data(resp);
        data.GetArrayLength().Should().BeGreaterThan(0);
        data[0].GetProperty("name").GetString().Should().Contain("Scotch");
    }

    [Fact]
    public async Task Products_Search_ByPriceRange_FiltersResults()
    {
        var resp = await _client.GetAsync("/api/products?minPrice=50&maxPrice=100&page=1&pageSize=50");
        var data = await Data(resp);
        foreach (var item in data.EnumerateArray())
        {
            var price = item.GetProperty("basePrice").GetDecimal();
            price.Should().BeGreaterOrEqualTo(50);
            price.Should().BeLessOrEqualTo(100);
        }
    }

    [Fact]
    public async Task Products_Featured_ReturnsFeaturedOnly()
    {
        var resp = await _client.GetAsync("/api/products/featured?count=5");
        var data = await Data(resp);
        data.GetArrayLength().Should().BeLessOrEqualTo(5);
        foreach (var item in data.EnumerateArray())
            item.GetProperty("isFeatured").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Products_GetBySku_ReturnsCorrectProduct()
    {
        var resp = await _client.GetAsync("/api/products/sku/PM-BS-001");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("name").GetString().Should().Be("Scotch Fillet Steak");
        data.GetProperty("sku").GetString().Should().Be("PM-BS-001");
    }

    [Fact]
    public async Task Products_GetBySku_InvalidSku_ReturnsNotFound()
    {
        var resp = await _client.GetAsync("/api/products/sku/INVALID-999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Products_GetById_InvalidId_ReturnsNotFound()
    {
        var resp = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Products_AllHaveImages()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        var data = await Data(resp);
        foreach (var product in data.EnumerateArray())
        {
            var url = product.GetProperty("primaryImageUrl").GetString();
            url.Should().NotBeNullOrEmpty(
                $"Product {product.GetProperty("name").GetString()} should have an image");
        }
    }

    [Fact]
    public async Task Products_ImageUrlsAreAccessible()
    {
        var resp = await _client.GetAsync("/api/products/featured?count=3");
        var data = await Data(resp);
        using var imgClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        foreach (var product in data.EnumerateArray())
        {
            var imgUrl = product.GetProperty("primaryImageUrl").GetString();
            if (!string.IsNullOrEmpty(imgUrl))
            {
                var imgResp = await imgClient.GetAsync(imgUrl);
                imgResp.StatusCode.Should().Be(HttpStatusCode.OK,
                    $"Image for {product.GetProperty("name").GetString()} should be accessible");
                imgResp.Content.Headers.ContentType?.MediaType.Should().StartWith("image/");
            }
        }
    }

    [Fact]
    public async Task Products_ScotchFillet_ExistsAndHasDetails()
    {
        var resp = await _client.GetAsync("/api/products/sku/PM-BS-001");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("name").GetString().Should().Contain("Scotch Fillet");
        data.GetProperty("basePrice").GetDecimal().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Products_Create_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/api/products",
            new { Name = "Test", SKU = "TEST-001", BasePrice = 9.99 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Products_HaveVariantsProperty()
    {
        var resp = await _client.GetAsync("/api/products/sku/PM-SB-004");
        var data = await Data(resp);
        data.TryGetProperty("variants", out _).Should().BeTrue("Product should have variants array");
    }

    [Fact]
    public async Task Products_AllSKUsStartWithPM()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        var data = await Data(resp);
        foreach (var p in data.EnumerateArray())
            p.GetProperty("sku").GetString().Should().StartWith("PM-");
    }

    // ═══════════════════════════════════════════════════
    //  CATEGORIES  (5 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Categories_GetAll_Returns25Categories()
    {
        var resp = await _client.GetAsync("/api/categories");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(25);
    }

    [Fact]
    public async Task Categories_Tree_ReturnsNestedStructure()
    {
        var resp = await _client.GetAsync("/api/categories/tree");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(8, "Should have 8 top-level categories");
    }

    [Fact]
    public async Task Categories_HaveImageUrls()
    {
        var resp = await _client.GetAsync("/api/categories");
        var data = await Data(resp);
        var withImages = data.EnumerateArray()
            .Count(c => c.TryGetProperty("imageUrl", out var img) &&
                        img.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrEmpty(img.GetString()));
        withImages.Should().BeGreaterOrEqualTo(6);
    }

    [Fact]
    public async Task Categories_GetById_Invalid_ReturnsNotFound()
    {
        var resp = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Categories_Create_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/api/categories", new { Name = "Test", SortOrder = 99 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════
    //  BRANDS  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Brands_GetAll_Returns6Brands()
    {
        var resp = await _client.GetAsync("/api/brands");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(6);
    }

    [Fact]
    public async Task Brands_IncludeExpectedNames()
    {
        var resp = await _client.GetAsync("/api/brands");
        var data = await Data(resp);
        var names = data.EnumerateArray().Select(b => b.GetProperty("name").GetString()).ToList();
        names.Should().Contain("Primo Cuts");
        names.Should().Contain("Heritage Reserve");
        names.Should().Contain("Grill Master");
    }

    [Fact]
    public async Task Brands_GetById_Invalid_ReturnsNotFound()
    {
        var resp = await _client.GetAsync($"/api/brands/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════
    //  DISCOUNTS  (6 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Discounts_GetActive_ReturnsDiscounts()
    {
        var resp = await _client.GetAsync("/api/discounts/active");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Discounts_Validate_BBQ20_WithSufficientAmount()
    {
        var resp = await _client.PostAsJsonAsync("/api/discounts/validate",
            new { code = "BBQ20", orderAmount = 50.00 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("isValid").GetBoolean().Should().BeTrue();
        data.GetProperty("discountAmount").GetDecimal().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Discounts_Validate_InvalidCode()
    {
        var resp = await _client.PostAsJsonAsync("/api/discounts/validate",
            new { code = "INVALID999", orderAmount = 100.00 });
        var data = await Data(resp);
        data.GetProperty("isValid").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Discounts_Validate_FIRSTORDER_BelowMin_Invalid()
    {
        var resp = await _client.PostAsJsonAsync("/api/discounts/validate",
            new { code = "FIRSTORDER", orderAmount = 10.00 });
        var data = await Data(resp);
        data.GetProperty("isValid").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Discounts_GetAll_RequiresAuth()
    {
        var resp = await _client.GetAsync("/api/discounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Discounts_GetAll_AdminCanAccess()
    {
        var resp = await GetAuth("/api/discounts", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(5);
    }

    // ═══════════════════════════════════════════════════
    //  VOUCHERS  (4 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Vouchers_Validate_MEAT25()
    {
        var resp = await PostAuth("/api/vouchers/validate",
            new { code = "MEAT25", orderAmount = 60.00 }, _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("isValid").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Vouchers_Validate_InvalidCode()
    {
        var resp = await PostAuth("/api/vouchers/validate",
            new { code = "FAKEVOUCHER", orderAmount = 100.00 }, _customerToken);
        var data = await Data(resp);
        data.GetProperty("isValid").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Vouchers_GetAll_RequiresAuth()
    {
        var resp = await _client.GetAsync("/api/vouchers");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Vouchers_GetAll_AdminCanAccess()
    {
        var resp = await GetAuth("/api/vouchers", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(5);
    }

    // ═══════════════════════════════════════════════════
    //  OFFERS  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Offers_GetAll_ReturnsOffers()
    {
        var resp = await _client.GetAsync("/api/offers");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Offers_GetActive_ReturnsActiveOffers()
    {
        var resp = await _client.GetAsync("/api/offers/active");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        foreach (var offer in data.EnumerateArray())
            offer.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Offers_HaveExpectedNames()
    {
        var resp = await _client.GetAsync("/api/offers");
        var data = await Data(resp);
        var names = data.EnumerateArray().Select(o => o.GetProperty("name").GetString()).ToList();
        names.Should().Contain("Weekend BBQ Pack");
    }

    // ═══════════════════════════════════════════════════
    //  CUSTOMERS  (6 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Customers_GetMe_AsCustomer_ReturnsProfile()
    {
        var resp = await GetAuth("/api/customers/me", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("firstName").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("addresses").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Customers_GetMe_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/customers/me");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customers_GetAll_AdminCanAccess()
    {
        var resp = await GetAuth("/api/customers?page=1&pageSize=10", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task Customers_GetAll_CustomerRole_ReturnsForbidden()
    {
        var resp = await GetAuth("/api/customers?page=1&pageSize=10", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Customers_AddressesAreAustralian()
    {
        var resp = await GetAuth("/api/customers?page=1&pageSize=10", _adminToken);
        var data = await Data(resp);
        foreach (var customer in data.EnumerateArray())
            foreach (var addr in customer.GetProperty("addresses").EnumerateArray())
                addr.GetProperty("country").GetString().Should().Be("AU");
    }

    [Fact]
    public async Task Customers_Loyalty_ReturnsLoyaltyInfo()
    {
        var meResp = await GetAuth("/api/customers/me", _customerToken);
        var meData = await Data(meResp);
        var customerId = meData.GetProperty("id").GetString();

        var resp = await GetAuth($"/api/customers/{customerId}/loyalty", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("loyaltyPoints").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    // ═══════════════════════════════════════════════════
    //  CART  (4 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Cart_GetCart_AsCustomer_ReturnsCart()
    {
        var resp = await GetAuth("/api/cart", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cart_GetCart_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/cart");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Cart_AddUpdateRemove_Flow()
    {
        // Get a product ID
        var prodResp = await _client.GetAsync("/api/products/sku/PM-BS-001");
        var productId = (await Data(prodResp)).GetProperty("id").GetString();

        // Add to cart
        var addResp = await PostAuth("/api/cart/items",
            new { productId, quantity = 1 }, _customerToken);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get cart to find item ID
        var cartResp = await GetAuth("/api/cart", _customerToken);
        var items = (await Data(cartResp)).GetProperty("items");
        items.GetArrayLength().Should().BeGreaterThan(0);
        var cartItemId = items[items.GetArrayLength() - 1].GetProperty("id").GetString();

        // Update quantity
        var updateResp = await PutAuth($"/api/cart/items/{cartItemId}",
            new { quantity = 3 }, _customerToken);
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Remove item
        var removeResp = await DeleteAuth($"/api/cart/items/{cartItemId}", _customerToken);
        removeResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cart_ClearCart_Works()
    {
        var resp = await DeleteAuth("/api/cart", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ═══════════════════════════════════════════════════
    //  ORDERS  (6 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Orders_GetAll_AdminCanAccess()
    {
        var resp = await GetAuth("/api/orders?page=1&pageSize=10", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().BeGreaterThan(0, "Should have seed orders");
    }

    [Fact]
    public async Task Orders_GetAll_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/orders?page=1&pageSize=10");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Orders_GetMyOrders_CustomerCanAccess()
    {
        var resp = await GetAuth("/api/orders/my?page=1&pageSize=10", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Orders_GetById_ValidOrder_ReturnsOrder()
    {
        var allResp = await GetAuth("/api/orders?page=1&pageSize=1", _adminToken);
        var orderId = (await Data(allResp))[0].GetProperty("id").GetString();

        var resp = await GetAuth($"/api/orders/{orderId}", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("orderNumber").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Orders_GetById_Invalid_ReturnsNotFound()
    {
        var resp = await GetAuth($"/api/orders/{Guid.NewGuid()}", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Orders_StatusHistory_ReturnsHistory()
    {
        var allResp = await GetAuth("/api/orders?page=1&pageSize=1", _adminToken);
        var orderId = (await Data(allResp))[0].GetProperty("id").GetString();

        var resp = await GetAuth($"/api/orders/{orderId}/history", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ═══════════════════════════════════════════════════
    //  PAYMENTS  (2 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Payments_GetByOrder_ReturnsPayments()
    {
        var allResp = await GetAuth("/api/orders?page=1&pageSize=10", _adminToken);
        var data = await Data(allResp);
        string? orderId = null;
        foreach (var order in data.EnumerateArray())
        {
            if (order.GetProperty("paymentStatus").GetInt32() == 1)
            { orderId = order.GetProperty("id").GetString(); break; }
        }
        if (orderId != null)
        {
            var resp = await GetAuth($"/api/payments/order/{orderId}", _adminToken);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Payments_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════
    //  REVIEWS  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Reviews_GetByProduct_ReturnsReviews()
    {
        var prodResp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        var products = await Data(prodResp);
        string? pid = null;
        foreach (var p in products.EnumerateArray())
        {
            if (p.GetProperty("totalReviews").GetInt32() > 0)
            { pid = p.GetProperty("id").GetString(); break; }
        }

        if (pid != null)
        {
            var resp = await _client.GetAsync($"/api/reviews/product/{pid}?page=1&pageSize=10");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);
            var data = await Data(resp);
            data.GetArrayLength().Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Reviews_CreateReview_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.PostAsJsonAsync("/api/reviews",
            new { productId = Guid.NewGuid(), rating = 5, title = "Test", comment = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reviews_SeedReviews_AreApproved()
    {
        var prodResp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        var products = await Data(prodResp);
        string? pid = null;
        foreach (var p in products.EnumerateArray())
        {
            if (p.GetProperty("totalReviews").GetInt32() > 0)
            { pid = p.GetProperty("id").GetString(); break; }
        }

        if (pid != null)
        {
            var resp = await _client.GetAsync($"/api/reviews/product/{pid}?page=1&pageSize=5");
            var data = await Data(resp);
            data[0].GetProperty("rating").GetInt32().Should().BeInRange(1, 5);
            data[0].GetProperty("isApproved").GetBoolean().Should().BeTrue();
        }
    }

    // ═══════════════════════════════════════════════════
    //  WISHLIST  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Wishlist_GetWishlist_AsCustomer()
    {
        var resp = await GetAuth("/api/wishlist", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Wishlist_AddAndRemove_Flow()
    {
        // Use a product unlikely to already be in the wishlist
        var prodResp = await _client.GetAsync("/api/products/sku/PM-BS-005");
        var productId = (await Data(prodResp)).GetProperty("id").GetString();

        // Remove first to ensure clean state (ignore result)
        await DeleteAuth($"/api/wishlist/{productId}", _customerToken);

        var addResp = await PostAuthEmpty($"/api/wishlist/{productId}", _customerToken);
        addResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

        if (addResp.StatusCode == HttpStatusCode.OK)
        {
            var checkResp = await GetAuth($"/api/wishlist/check/{productId}", _customerToken);
            checkResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        var removeResp = await DeleteAuth($"/api/wishlist/{productId}", _customerToken);
        removeResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Wishlist_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/wishlist");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════
    //  INVENTORY  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Inventory_LowStock_AdminCanAccess()
    {
        var resp = await GetAuth("/api/inventory/low-stock?threshold=5", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Inventory_LowStock_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/inventory/low-stock?threshold=5");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Inventory_GetTransactions_AdminCanAccess()
    {
        var prodResp = await _client.GetAsync("/api/products/sku/PM-BS-001");
        var productId = (await Data(prodResp)).GetProperty("id").GetString();

        var resp = await GetAuth($"/api/inventory/product/{productId}?page=1&pageSize=10", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ═══════════════════════════════════════════════════
    //  DASHBOARD  (6 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Dashboard_SalesSummary_AdminCanAccess()
    {
        var resp = await GetAuth("/api/dashboard/sales-summary", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("totalOrders").GetInt32().Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Dashboard_TopProducts_AdminCanAccess()
    {
        var resp = await GetAuth("/api/dashboard/top-products?count=5", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dashboard_CustomerStats_AdminCanAccess()
    {
        var resp = await GetAuth("/api/dashboard/customer-stats", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("totalCustomers").GetInt32().Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task Dashboard_RevenueChart_AdminCanAccess()
    {
        var resp = await GetAuth("/api/dashboard/revenue-chart?from=2020-01-01&to=2030-12-31", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Dashboard_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/dashboard/sales-summary");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Dashboard_CustomerRole_ReturnsForbidden()
    {
        var resp = await GetAuth("/api/dashboard/sales-summary", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ═══════════════════════════════════════════════════
    //  SETTINGS  (6 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Settings_GetAll_AdminCanAccess()
    {
        var resp = await GetAuth("/api/settings", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(7);
    }

    [Fact]
    public async Task Settings_GetByKey_StoreName()
    {
        var resp = await GetAuth("/api/settings/Store.Name", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("value").GetString().Should().Be("Primo Meats");
    }

    [Fact]
    public async Task Settings_GetByKey_Currency()
    {
        var resp = await GetAuth("/api/settings/Store.Currency", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetProperty("value").GetString().Should().Be("AUD");
    }

    [Fact]
    public async Task Settings_GetByGroup_General()
    {
        var resp = await GetAuth("/api/settings/group/General", _adminToken);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await Data(resp);
        data.GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Settings_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync("/api/settings");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Settings_CustomerRole_ReturnsForbidden()
    {
        var resp = await GetAuth("/api/settings", _customerToken);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ═══════════════════════════════════════════════════
    //  IMAGES  (2 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task Images_Upload_WithoutAuth_ReturnsUnauthorized()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");
        var resp = await _client.PostAsync("/api/images/upload?folder=test", content);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Images_Delete_WithoutAuth_ReturnsUnauthorized()
    {
        var resp = await _client.DeleteAsync("/api/images?publicId=test/image");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════
    //  RESPONSE ENVELOPE  (2 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task ResponseEnvelope_HasCorrectStructure()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=1");
        var root = await Root(resp);
        root.TryGetProperty("success", out _).Should().BeTrue();
        root.TryGetProperty("message", out _).Should().BeTrue();
        root.TryGetProperty("data", out _).Should().BeTrue();
        root.TryGetProperty("errors", out _).Should().BeTrue();
        root.TryGetProperty("timestamp", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ResponseEnvelope_PaginatedEndpoint_HasPagination()
    {
        var resp = await _client.GetAsync("/api/products?page=2&pageSize=5");
        var root = await Root(resp);
        var pg = root.GetProperty("pagination");
        pg.GetProperty("currentPage").GetInt32().Should().Be(2);
        pg.GetProperty("pageSize").GetInt32().Should().Be(5);
        pg.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        pg.GetProperty("totalPages").GetInt32().Should().BeGreaterThan(0);
        pg.GetProperty("hasPrevious").GetBoolean().Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════
    //  DATA INTEGRITY  (3 tests)
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task DataIntegrity_AllProductsHaveCategoryAndBrand()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        foreach (var p in (await Data(resp)).EnumerateArray())
        {
            p.GetProperty("categoryName").GetString().Should().NotBeNullOrEmpty();
            p.GetProperty("brandName").GetString().Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task DataIntegrity_AllProductsHaveValidPricing()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=74");
        foreach (var p in (await Data(resp)).EnumerateArray())
        {
            p.GetProperty("basePrice").GetDecimal().Should().BeGreaterThan(0);
            p.GetProperty("stockQuantity").GetInt32().Should().BeGreaterOrEqualTo(0);
        }
    }

    [Fact]
    public async Task DataIntegrity_ProductCount_Is74()
    {
        var resp = await _client.GetAsync("/api/products?page=1&pageSize=1");
        var root = await Root(resp);
        root.GetProperty("pagination").GetProperty("totalCount").GetInt32().Should().Be(74);
    }

    // ═══════════════════════════════════════════════════
    //  E2E: Browse → Cart → Order Flow
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task E2E_BrowseToOrder_FullFlow()
    {
        // 1. Browse products (public)
        var browseResp = await _client.GetAsync("/api/products/featured?count=3");
        browseResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await Data(browseResp);
        var firstProduct = products[0];
        var productId = firstProduct.GetProperty("id").GetString();
        firstProduct.GetProperty("primaryImageUrl").GetString().Should().NotBeNullOrEmpty();

        // 2. View categories (public)
        var catResp = await _client.GetAsync("/api/categories/tree");
        catResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Add to cart (customer)
        var addResp = await PostAuth("/api/cart/items",
            new { productId, quantity = 2 }, _customerToken);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. View cart
        var cartResp = await GetAuth("/api/cart", _customerToken);
        cartResp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Data(cartResp)).GetProperty("total").GetDecimal().Should().BeGreaterThan(0);

        // 5. Validate a discount
        var discountResp = await _client.PostAsJsonAsync("/api/discounts/validate",
            new { code = "FIRSTORDER", orderAmount = 50.00 });
        discountResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Cleanup: clear cart
        await DeleteAuth("/api/cart", _customerToken);
    }
}
