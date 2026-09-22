using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace ECommerce.Tests.Integration;

/// <summary>Public catalog endpoint against the real API pipeline (seeded demo data).</summary>
public sealed class ProductsApiTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsApiTests()
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
    public async Task GetProducts_ReturnsOk_WithApiResponseEnvelope()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        root.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        var data = root.GetProperty("data");
        data.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
        data.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        data.GetProperty("totalPages").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProducts_WithSearchFilter_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products?search=headphones&page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }
}
