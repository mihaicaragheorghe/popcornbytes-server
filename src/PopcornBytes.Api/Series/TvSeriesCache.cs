using System.Text.Json;

using StackExchange.Redis;

namespace PopcornBytes.Api.Series;

public class TvSeriesCache : ITvSeriesCache
{
    private readonly IDatabase _redis;

    public TvSeriesCache(IConnectionMultiplexer mutex)
    {
        _redis = mutex.GetDatabase();
    }

    public async Task<TvSeries?> Get(int id)
    {
        RedisValue val = await _redis.StringGetAsync(Key(id));

        return val.HasValue ? JsonSerializer.Deserialize<TvSeries>(val!) : null;
    }

    public async Task<List<TvSeries>> Get(IEnumerable<int> ids)
    {
        var keys = ids.Select(id => (RedisKey)Key(id)).ToArray();
        var results = await _redis.StringGetAsync(keys);

        return keys
            .Zip(results)
            .Where(p => p.Second.HasValue)
            .Select(p => JsonSerializer.Deserialize<TvSeries>(p.Second!)!)
            .ToList();
    }

    public Task<bool> Set(TvSeries series)
    {
        string key = Key(series.Id);
        TimeSpan ttl = GetTTL(series);
        string json = JsonSerializer.Serialize(series);

        return _redis.StringSetAsync(key, json, ttl);
    }

    internal static string Key(int id) => $"series:{id}";

    private static TimeSpan GetTTL(TvSeries series)
    {
        if (!series.InProduction)
        {
            return TimeSpan.FromDays(14);
        }

        DateTime? release = series.NextEpisode?.ReleaseDate;
        if (release is null)
        {
            return TimeSpan.FromDays(5);
        }

        var daysUntilNext = (release.Value - DateTime.Today).TotalDays;

        return daysUntilNext switch
        {
            <= 1 => TimeSpan.FromHours(1),
            <= 7 => TimeSpan.FromHours(6),
            <= 30 => TimeSpan.FromDays(1),
            <= 60 => TimeSpan.FromDays(2),
            _ => TimeSpan.FromDays(5)
        };
    }
}
