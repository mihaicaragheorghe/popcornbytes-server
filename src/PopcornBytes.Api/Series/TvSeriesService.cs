using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ILogger<TvSeriesService> _logger;
    private readonly ICacheService<int, TvSeries> _cache;
    private readonly ITmdbClient _tmdbClient;

    public TvSeriesService(
        ILogger<TvSeriesService> logger,
        ICacheService<int, TvSeries> cache,
        ITmdbClient tmdbClient)
    {
        _tmdbClient = tmdbClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(id, out TvSeries cachedSeries))
        {
            _logger.LogInformation("Cache hit for TV series with ID {id}", id);
            return cachedSeries;
        }

        var tmdbResponse = await _tmdbClient.GetTvSeriesAsync(id, cancellationToken);
        var series = tmdbResponse?.ToTvSeries();
        if (series == null) return null;
        _cache.Set(id, series);

        return series;
    }
}