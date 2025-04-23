using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ILogger<TvSeriesService> _logger;
    private readonly ITvSeriesCache _cache;
    private readonly ITmdbClient _tmdbClient;

    public TvSeriesService(
        ILogger<TvSeriesService> logger,
        ITvSeriesCache cache,
        ITmdbClient tmdbClient)
    {
        _logger = logger;
        _cache = cache;
        _tmdbClient = tmdbClient;
    }

    public async Task<TvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default)
    {
        var cachedSeries = await _cache.Get(id);
        if (cachedSeries != null)
        {
            _logger.LogDebug("Cache hit for TV series with ID {id}", id);
            return cachedSeries;
        }

        var tmdbResponse = await _tmdbClient.GetTvSeriesAsync(id, cancellationToken);
        if (tmdbResponse is null) return null;

        var series = TvSeries.FromTmdbSeries(tmdbResponse);
        bool ok = await _cache.Set(series);
        if (!ok)
        {
            _logger.LogError("Could not cache series {id}", id);
        }

        return series;
    }
}