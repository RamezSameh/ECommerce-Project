using System.Collections.Concurrent;
using System.Text.Json;
using ECommerce.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Redis-backed implementation of <see cref="ICacheService"/>.
/// <para>
/// The connection string is read from <c>ConnectionStrings:Redis</c>, falling back to
/// <c>Cache:Redis:ConnectionString</c>. When no connection string is configured, or Redis
/// is unreachable, every operation transparently falls back to an in-memory cache
/// (same behavior as <see cref="InMemoryCacheService"/>) instead of throwing.
/// </para>
/// </summary>
public class RedisCacheService : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

    // Multiplexers are designed to be shared; one connection per connection string,
    // reused by every instance of this service.
    private static readonly ConcurrentDictionary<string, Lazy<Task<ConnectionMultiplexer?>>> _connections = new();

    private readonly InMemoryCacheService _fallback;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly Task<ConnectionMultiplexer?> _multiplexer;

    public RedisCacheService(
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<RedisCacheService> logger)
    {
        _fallback = new InMemoryCacheService(memoryCache);
        _logger = logger;
        var connectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Cache:Redis:ConnectionString"];
        // Connection attempt happens once, lazily, on first cache operation.
        _multiplexer = string.IsNullOrWhiteSpace(connectionString)
            ? LogNoRedisAndFallback()
            : _connections.GetOrAdd(connectionString,
                cs => new Lazy<Task<ConnectionMultiplexer?>>(() => ConnectAsync(cs))).Value;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var db = await GetDatabaseAsync();
        if (db is null) return await _fallback.GetAsync<T>(key);

        try
        {
            var value = await db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T?>(value.ToString());
        }
        catch (Exception ex) when (ex is RedisException)
        {
            _logger.LogWarning(ex, "Redis GET failed for key {Key}; falling back to in-memory cache.", key);
            return await _fallback.GetAsync<T>(key);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var db = await GetDatabaseAsync();
        if (db is null)
        {
            await _fallback.SetAsync(key, value, ttl);
            return;
        }

        try
        {
            await db.StringSetAsync(key, JsonSerializer.Serialize(value), ttl ?? DefaultTtl);
        }
        catch (Exception ex) when (ex is RedisException)
        {
            _logger.LogWarning(ex, "Redis SET failed for key {Key}; falling back to in-memory cache.", key);
            await _fallback.SetAsync(key, value, ttl);
        }
    }

    public async Task RemoveAsync(string key)
    {
        var db = await GetDatabaseAsync();
        if (db is null)
        {
            await _fallback.RemoveAsync(key);
            return;
        }

        try
        {
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex) when (ex is RedisException)
        {
            _logger.LogWarning(ex, "Redis DEL failed for key {Key}; removing from in-memory cache.", key);
            await _fallback.RemoveAsync(key);
        }
    }

    private Task<ConnectionMultiplexer?> LogNoRedisAndFallback()
    {
        _logger.LogInformation("No Redis connection string configured; using in-memory cache fallback.");
        return Task.FromResult<ConnectionMultiplexer?>(null);
    }

    private async Task<IDatabase?> GetDatabaseAsync()
        => (await _multiplexer)?.GetDatabase();

    private async Task<ConnectionMultiplexer?> ConnectAsync(string connectionString)
    {
        try
        {
            var multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            _logger.LogInformation("Connected to Redis cache.");
            return multiplexer;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not connect to Redis; using in-memory cache fallback.");
            return null;
        }
    }
}
