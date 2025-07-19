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

    public async Task<SearchTvSeriesResponse> QueryAsync(string query, int page = 1,
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

    public async Task<TvSeries?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
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

    public Task TrackAsync(Guid userId, int seriesId, TrackedSeriesState state)
    {
        _logger.LogDebug("Adding series {seriesId} to {state} for user {userId}", seriesId, state, userId);

        long addedAtUnix = DateTime.UtcNow.ToUnixTime();

        return state switch
        {
            TrackedSeriesState.Watchlist => _seriesRepository.AddToWatchlistAsync(userId, seriesId, addedAtUnix),
            TrackedSeriesState.Watching => _seriesRepository.AddToWatchingAsync(userId, seriesId, addedAtUnix),
            TrackedSeriesState.Completed => _seriesRepository.AddToCompletedAsync(userId, seriesId, addedAtUnix),
            TrackedSeriesState.Stopped => throw new ArgumentOutOfRangeException(nameof(state)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public Task RemovedTrackedAsync(Guid userId, int seriesId, TrackedSeriesState state)
    {
        _logger.LogDebug("Deleting series {seriesId} from {state} for user {userId}", seriesId, state, userId);

        return state switch
        {
            TrackedSeriesState.Watchlist => _seriesRepository.RemoveFromWatchlistAsync(userId, seriesId),
            TrackedSeriesState.Watching => _seriesRepository.RemoveFromWatchingAsync(userId, seriesId),
            TrackedSeriesState.Completed => _seriesRepository.RemoveFromCompletedAsync(userId, seriesId),
            TrackedSeriesState.Stopped => throw new ArgumentOutOfRangeException(nameof(state)),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    public async Task<List<TvSeries>> GetTrackedAsync(Guid userId, TrackedSeriesState state,
        CancellationToken cancellationToken = default)
    {
        var ids = (await GetTrackedSeriesIdsAsync(userId, state)).ToList();

        if (ids.Count > 0)
        {
            return await GetFromCacheOrTmdbAsync(ids, cancellationToken);
        }

        _logger.LogDebug("No watchlist entries found for user {userId}", userId);
        return [];
    }

    private Task<IEnumerable<int>> GetTrackedSeriesIdsAsync(Guid userId, TrackedSeriesState state) => state switch
    {
        TrackedSeriesState.Watchlist => _seriesRepository.GetWatchlistAsync(userId),
        TrackedSeriesState.Watching => _seriesRepository.GetWatchingAsync(userId),
        TrackedSeriesState.Completed => _seriesRepository.GetCompletedAsync(userId),
        TrackedSeriesState.Stopped => _seriesRepository.GetStoppedAsync(userId),
        _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
    };

    public async Task<Result> StopWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Stopping watching series {s} for user {u}", seriesId, userId);

        var rowsAffected = await _seriesRepository.StopWatchingAsync(userId, seriesId, DateTime.UtcNow.ToUnixTime());

        return rowsAffected == 0
            ? Error.Failure(code: "series.not_started", message: "The series was not started")
            : Result.Success();
    }

    public async Task<Result> ResumeWatching(Guid userId, int seriesId)
    {
        _logger.LogDebug("Stopping watching series {s} for user {u}", seriesId, userId);

        var rowsAffected = await _seriesRepository.ResumeWatchingAsync(userId, seriesId, DateTime.UtcNow.ToUnixTime());

        return rowsAffected == 0
            ? Error.Failure(code: "series.not_stopped", message: "The series was not stopped")
            : Result.Success();
    }

    private async Task<List<TvSeries>> GetFromCacheOrTmdbAsync(List<int> ids,
        CancellationToken cancellationToken)
    {
        var cachedSeries = await _cache.Get(ids);
        var missingIds = ids.Except(cachedSeries.Select(s => s.Id)).ToList();

        if (missingIds.Count == 0)
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