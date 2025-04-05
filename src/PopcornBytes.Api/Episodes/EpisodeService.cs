using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Episodes;

public class EpisodeService : IEpisodeService
{
    private readonly ILogger<EpisodeService> _logger;
    private readonly ITmdbClient _tmdbClient;
    private readonly ICacheService<string, List<Episode>> _cache;

    public EpisodeService(
        ILogger<EpisodeService> logger,
        ITmdbClient tmdbClient,
        ICacheService<string, List<Episode>> cache)
    {
        _logger = logger;
        _tmdbClient = tmdbClient;
        _cache = cache;
    }

    public async Task<List<Episode>?> GetEpisodesAsync(int seriesId, int seasonNumber,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey(seriesId, seasonNumber), out var cachedEpisodes))
        {
            _logger.LogDebug("Cache hit for episodes series {id}, season {s}", seriesId, seasonNumber);
            return cachedEpisodes;
        }
        
        var tmdbResponse = await _tmdbClient.GetEpisodesAsync(seriesId, seasonNumber, cancellationToken);
        if (tmdbResponse is null) return null;
        
        var episodes = tmdbResponse.Select(Episode.FromTmdbEpisode).ToList();
        _cache.Set(CacheKey(seriesId, seasonNumber), episodes);
        return episodes;
    }

    public async Task<Episode?> GetEpisodeAsync(int seriesId, int seasonNumber, int episodeNumber,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey(seriesId, seasonNumber), out var cachedEpisodes))
        {
            _logger.LogDebug("Cache hit for series {id}, season {s}", seriesId, seasonNumber);
            return cachedEpisodes.FirstOrDefault(e => e.EpisodeNumber == episodeNumber);
        }
        
        var tmdbResponse = await _tmdbClient.GetEpisodeAsync(seriesId, seasonNumber, episodeNumber, cancellationToken);
        if (tmdbResponse is null) return null;
        
        return Episode.FromTmdbEpisode(tmdbResponse);
    }

    private string CacheKey(int seriesId, int seasonNumber)
    {
        return $"{seriesId}-{seasonNumber}";
    }
}