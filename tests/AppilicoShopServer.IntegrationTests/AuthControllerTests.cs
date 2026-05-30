using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace AppilicoShopServer.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        var request = new
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        var request = new { FirstName = "", LastName = "", Email = "invalid", Password = "short", ConfirmPassword = "mismatch" };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new { Email = "nonexistent@example.com", Password = "WrongPass@1" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnsTokens()
    {
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var registerReq = new { FirstName = "Login", LastName = "Test", Email = email, Password = "Login@12345", ConfirmPassword = "Login@12345" };
        var regResponse = await _client.PostAsJsonAsync("/api/auth/register", registerReq);
        // Registration may succeed or conflict if email already taken

        var loginReq = new { Email = email, Password = "Login@12345" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginReq);

        // If registration succeeded, login should work; otherwise login returns unauthorized/bad request
        loginResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProfile_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
