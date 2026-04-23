using CommentsApp.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

public class CacheService: ICacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _redis;
    public CacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if(value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(value!);
    } 

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        var json=JsonSerializer.Serialize<T>(value);
        await _database.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server.Keys(pattern: $"{prefix}*").ToArray();

        foreach (var key in keys)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}