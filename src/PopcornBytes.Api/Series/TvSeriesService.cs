using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ILogger<TvSeriesService> _logger;
    private readonly ITmdbClient _tmdbClient;
    private readonly ITvSeriesCache _cache;

    public TvSeriesService(ILogger<TvSeriesService> logger, ITmdbClient tmdbClient, ITvSeriesCache cache)
    {
        _tmdbClient = tmdbClient;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<TvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
    {
        var cached = _cache.Get(id);
        if (cached != null)
        {
            _logger.LogInformation("Cache hit for TV series with ID {id}", id);
            return cached;
        }
        
        var tmdbResponse = await _tmdbClient.GetTvSeriesAsync(id, cancellationToken);
        var series = tmdbResponse?.ToTvSeries();
        if (series == null) return null;
         _cache.Set(id, series);
        
        return series;
    }
}