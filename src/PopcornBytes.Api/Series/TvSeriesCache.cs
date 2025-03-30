using PopcornBytes.Api.Kernel;

namespace PopcornBytes.Api.Series;

public class TvSeriesCache : ITvSeriesCache
{
    private readonly LRUCache<int, TvSeries> _cache;

    public TvSeriesCache(int capacity, int expirationInHours)
    {
        _cache = new LRUCache<int, TvSeries>(capacity, TimeSpan.FromHours(expirationInHours));
    }
    
    public TvSeries? Get(int id) => _cache.TryGetValue(id, out TvSeries? series) ? series : null;

    public void Set(int id, TvSeries series) => _cache.Set(id, series);
}
