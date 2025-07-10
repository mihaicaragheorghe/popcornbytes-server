using PopcornBytes.Api.Extensions;
using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ILogger<TvSeriesService> _logger;
    private readonly ITvSeriesCache _cache;
    private readonly ICacheService<string, SearchTvSeriesResponse> _searchCache;
    private readonly ITvSeriesRepository _seriesRepository;
    private readonly ITmdbClient _tmdbClient;

    public TvSeriesService(
        ILogger<TvSeriesService> logger,
        ITmdbClient tmdbClient,
        ITvSeriesCache cache,
        ICacheService<string, SearchTvSeriesResponse> searchCache,
        ITvSeriesRepository seriesRepository)
    {
        _logger = logger;
        _tmdbClient = tmdbClient;
        _cache = cache;
        _searchCache = searchCache;
        _seriesRepository = seriesRepository;
    }

    public async Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (_searchCache.TryGetValue(SearchCacheKey(query, page), out SearchTvSeriesResponse cached))
        {
            _logger.LogDebug("Cache hit for tv series query {q}, page {p}", query, page);
            return cached;
        }

        var tmdbResponse = await _tmdbClient.SearchTvSeriesAsync(query, page, cancellationToken);
        _searchCache.Set(SearchCacheKey(query, page), tmdbResponse);
        return tmdbResponse;
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

        var series = await GetFromTmdbAsync(id, cancellationToken);

        return series;
    }

    public Task AddToWatchlist(Guid userId, int seriesId)
    {
        _logger.LogDebug("Adding series {s} to watchlist for user {u}", seriesId, userId);

        long addedAtUnix = DateTime.UtcNow.ToUnixTime();

        return _seriesRepository.AddToWatchlistAsync(userId, seriesId, addedAtUnix);
    }

    public Task RemoveFromWatchlist(Guid userId, int seriesId)
    {
        return _seriesRepository.RemoveFromWatchlistAsync(userId, seriesId);
    }

    public async Task<List<TvSeries>> GetWatchlistAsync(Guid userId, CancellationToken cancellationToken)
    {
        var ids = await _seriesRepository.GetWatchlistAsync(userId);
        if (!ids.Any())
        {
            _logger.LogDebug("No watchlist entries found for user {userId}", userId);
            return [];
        }

        return await GetFromCacheOrTmdbAsync(ids, cancellationToken);
    }

    public Task AddToCompleted(Guid userId, int seriesId)
    {
        _logger.LogDebug("Adding series {s} to completed for user {u}", seriesId, userId);

        long completedAtUnix = DateTime.UtcNow.ToUnixTime();

        return _seriesRepository.AddToCompletedAsync(userId, seriesId, completedAtUnix);
    }

    public Task RemoveFromCompleted(Guid userId, int seriesId)
    {
        return _seriesRepository.RemoveFromCompletedAsync(userId, seriesId);
    }

    public async Task<List<TvSeries>> GetCompletedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var ids = await _seriesRepository.GetCompletedAsync(userId);
        if (!ids.Any())
        {
            _logger.LogDebug("No completed series found for user {userId}", userId);
            return [];
        }

        return await GetFromCacheOrTmdbAsync(ids, cancellationToken);
    }

    public Task AddToWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Adding series {s} to watching for user {u}", seriesId, userId);
        return _seriesRepository.AddToWatchingAsync(userId, seriesId, DateTime.UtcNow.ToUnixTime());
    }

    public Task StopWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Stopping watching series {s} for user {u}", seriesId, userId);
        return _seriesRepository.StopWatchingAsync(userId, seriesId, DateTime.UtcNow.ToUnixTime());
    }

    public Task ResumeWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Resuming watching series {s} for user {u}", seriesId, userId);
        return _seriesRepository.ResumeWatchingAsync(userId, seriesId, DateTime.UtcNow.ToUnixTime());
    }

    public Task RemoveFromWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Removing series {s} from watching for user {u}", seriesId, userId);
        return _seriesRepository.RemoveFromWatchingAsync(userId, seriesId);
    }

    public async Task<List<TvSeries>> GetWatchingAsync(Guid userId, CancellationToken cancellationToken)
    {
        var ids = await _seriesRepository.GetWatchingAsync(userId);
        if (!ids.Any())
        {
            _logger.LogDebug("No watching series found for user {userId}", userId);
            return [];
        }

        return await GetFromCacheOrTmdbAsync(ids, cancellationToken);
    }

    private async Task<List<TvSeries>> GetFromCacheOrTmdbAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var cachedSeries = await _cache.Get(ids);
        var missingIds = ids.Except(cachedSeries.Select(s => s.Id));

        if (!missingIds.Any())
        {
            _logger.LogDebug("All series found in cache for IDs: {ids}", string.Join(", ", ids));
            return cachedSeries;
        }

        foreach (var id in missingIds)
        {
            var series = await GetFromTmdbAsync(id, cancellationToken);
            if (series != null)
            {
                cachedSeries.Add(series);
            }
        }

        _logger.LogDebug("Fetched {count} series from TMDB and cached them", cachedSeries.Count);
        return cachedSeries;
    }

    private async Task<TvSeries?> GetFromTmdbAsync(int id, CancellationToken cancellationToken)
    {
        // TODO: TmdbClient to use results
        var tmdbSeries = await _tmdbClient.GetTvSeriesAsync(id, cancellationToken);
        if (tmdbSeries == null)
        {
            _logger.LogWarning("No TMDB series found for ID {id}", id);
            return null;
        }

        var series = TvSeries.FromTmdbSeries(tmdbSeries);
        bool ok = await _cache.Set(series);
        if (!ok)
        {
            _logger.LogError("Could not cache series {id}", id);
        }

        return series;
    }

    private static string SearchCacheKey(string query, int page) => $"{query}:{page}";
}