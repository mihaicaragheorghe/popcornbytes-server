using PopcornBytes.Api.Tmdb;

namespace PopcornBytes.Api.Episodes;

public class EpisodeService : IEpisodeService
{
    private readonly ILogger<EpisodeService> _logger;
    private readonly ITmdbClient _tmdbClient;
    private readonly IEpisodesCache _cache;

    public EpisodeService(
        ILogger<EpisodeService> logger,
        ITmdbClient tmdbClient,
        IEpisodesCache cache)
    {
        _logger = logger;
        _tmdbClient = tmdbClient;
        _cache = cache;
    }

    public async Task<List<Episode>?> GetEpisodesAsync(int seriesId, int seasonNumber,
        CancellationToken cancellationToken = default)
    {
        var cachedEpisodes = await _cache.Get(seriesId, seasonNumber);
        if (cachedEpisodes != null)
        {
            _logger.LogDebug("Cache hit for episodes series {id}, season {s}", seriesId, seasonNumber);
            return cachedEpisodes;
        }

        var tmdbResponse = await _tmdbClient.GetEpisodesAsync(seriesId, seasonNumber, cancellationToken);
        if (tmdbResponse is null) return null;

        var episodes = tmdbResponse.Select(Episode.FromTmdbEpisode).ToList();
        bool ok = await _cache.Set(episodes);
        if (!ok)
        {
            _logger.LogError("Could not cache episodes for series ID {sid}, season {s}", seriesId, seasonNumber);
        }

        return episodes;
    }

    public async Task<Episode?> GetEpisodeAsync(int seriesId, int seasonNumber, int episodeNumber,
        CancellationToken cancellationToken = default)
    {
        var cachedEpisodes = await _cache.Get(seriesId, seasonNumber);
        if (cachedEpisodes != null)
        {
            _logger.LogDebug("Cache hit for episodes series {id}, season {s}", seriesId, seasonNumber);
            return cachedEpisodes.FirstOrDefault(e => e.EpisodeNumber == episodeNumber);
        }

        var tmdbResponse = await _tmdbClient.GetEpisodeAsync(seriesId, seasonNumber, episodeNumber, cancellationToken);
        if (tmdbResponse is null) return null;

        return Episode.FromTmdbEpisode(tmdbResponse);
    }
}