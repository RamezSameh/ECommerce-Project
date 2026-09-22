using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace ECommerce.Tests.Integration;

/// <summary>End-to-end auth flow against the real API pipeline.</summary>
public sealed class AuthApiTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthApiTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateHttpsClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccessEnvelope()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = "testuser",
            email = $"test-{Guid.NewGuid():N}@example.com",
            password = "Test@1234",
            fullName = "Test User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnsJwtToken()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = "loginuser",
            email,
            password = "Test@1234",
            fullName = "Login User"
        });
        register.EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Test@1234"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        root.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        root.GetProperty("data").GetProperty("token").GetString()
            .Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"wrongpass-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = "wrongpassuser",
            email,
            password = "Test@1234"
        });
        register.EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "Wrong@9999"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
