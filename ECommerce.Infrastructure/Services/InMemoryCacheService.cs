using System.Text.Json;
using ECommerce.Application.Services;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Simple in-memory cache implementation. Production can substitute a Redis
/// implementation of the same interface without changing callers.
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out object? cached);
        return Task.FromResult(cached is null ? default : JsonSerializer.Deserialize<T?>(cached.ToString()!));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var json = JsonSerializer.Serialize(value);
        _cache.Set(key, json, ttl ?? TimeSpan.FromMinutes(10));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}