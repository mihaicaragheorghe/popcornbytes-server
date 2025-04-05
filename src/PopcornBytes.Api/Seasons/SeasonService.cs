using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Series;
using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Seasons;

public class SeasonService : ISeasonService
{
    private readonly ILogger<SeasonService> _logger;
    private readonly ICacheService<int, TvSeries> _cache;
    private readonly ITmdbClient _tmdbClient;

    public SeasonService(
        ILogger<SeasonService> logger,
        ICacheService<int, TvSeries> cache,
        ITmdbClient tmdbClient)
    {
        _tmdbClient = tmdbClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Season?> GetSeasonAsync(int seriesId, int seasonNumber,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(seriesId, out TvSeries cachedSeries))
        {
            _logger.LogDebug("Cache hit for TV series with ID {id} on season lookup", seriesId);
            return cachedSeries.Seasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber);
        }

        var tmdbResponse = await _tmdbClient.GetSeasonAsync(seriesId, seasonNumber, cancellationToken);
        if (tmdbResponse is null) return null;

        var series = Season.FromTmdbSeason(tmdbResponse, seriesId);
        return series;
    }
}