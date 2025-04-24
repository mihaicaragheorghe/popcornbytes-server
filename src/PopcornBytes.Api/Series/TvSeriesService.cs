using PopcornBytes.Api.Kernel;
using PopcornBytes.Api.Tmdb;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

public class TvSeriesService : ITvSeriesService
{
    private readonly ILogger<TvSeriesService> _logger;
    private readonly ITvSeriesCache _cache;
    private readonly ICacheService<string, SearchTvSeriesResponse> _searchCache;
    private readonly ITmdbClient _tmdbClient;

    public TvSeriesService(
        ILogger<TvSeriesService> logger,
        ITmdbClient tmdbClient,
        ITvSeriesCache cache,
        ICacheService<string, SearchTvSeriesResponse> searchCache)
    {
        _logger = logger;
        _tmdbClient = tmdbClient;
        _cache = cache;
        _searchCache = searchCache;
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

        var series = TvSeries.FromTmdbSeries(tmdbResponse);
        bool ok = await _cache.Set(series);
        if (!ok)
        {
            _logger.LogError("Could not cache series {id}", id);
        }

        return series;
    }
    
    private static string SearchCacheKey(string query, int page) => $"{query}:{page}";
}