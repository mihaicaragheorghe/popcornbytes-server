using System.Text.Json;

using PopcornBytes.Api.Series;

using StackExchange.Redis;

namespace PopcornBytes.Api.Episodes;

public class EpisodesCache : IEpisodesCache
{
    private readonly IDatabase _db;

    public EpisodesCache(IConnectionMultiplexer mutex)
    {
        _db = mutex.GetDatabase();
    }

    public async Task<List<Episode>?> Get(int seriesId, int seasonNum)
    {
        var val = await _db.StringGetAsync(Key(seriesId, seasonNum));

        return val.HasValue ? JsonSerializer.Deserialize<List<Episode>>(val!) : null;
    }

    public async Task<bool> Set(IEnumerable<Episode> episodes)
    {
        var setTasks = new List<Task<bool>>();
        var grouped = episodes.GroupBy(e => (e.SeriesId, e.SeasonNumber));

        foreach (var group in grouped)
        {
            string key = Key(seriesId: group.Key.SeriesId, seasonNum: group.Key.SeasonNumber);
            string value = JsonSerializer.Serialize(group.ToList());
            TimeSpan ttl = await _db.KeyTimeToLiveAsync(TvSeriesCache.Key(group.Key.SeriesId)) ?? TimeSpan.FromDays(1);
            
            setTasks.Add(_db.StringSetAsync(key, value, ttl));
        }

        bool[] results = await Task.WhenAll(setTasks);
        return results.All(r => r);
    }

    private static string Key(int seriesId, int seasonNum) => $"series:{seriesId}:{seasonNum}";

    private static string Key(Episode ep) => Key(ep.SeriesId, ep.SeasonNumber);
}