using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace ECommerce.Tests.Integration;

/// <summary>Verifies that protected endpoints reject anonymous callers.</summary>
public sealed class AuthorizationApiTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthorizationApiTests()
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
    public async Task Checkout_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/orders/checkout", new
        {
            paymentMethod = 0,
            shippingAddress = "Cairo, Egypt",
            phone = "01000000000"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MyOrders_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/orders/my-orders");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Checkout_WithToken_ReachesBusinessLogic()
    {
        // register + login to obtain a token, then call the protected endpoint:
        // it must NOT return 401 anymore (empty cart -> 400 from business logic).
        var email = $"checkout-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            userName = "checkoutuser",
            email,
            password = "Test@1234"
        });
        register.EnsureSuccessStatusCode();

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Test@1234" });
        using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("data").GetProperty("token").GetString();
        token.Should().NotBeNullOrWhiteSpace();

        using var authed = _factory.CreateHttpsClient();
        authed.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await authed.PostAsJsonAsync("/api/orders/checkout", new
        {
            paymentMethod = 0,
            shippingAddress = "Cairo, Egypt",
            phone = "01000000000"
        });

        // 400 = authenticated, but the cart is empty (business rule) — proves auth passed.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
