using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

public interface ITvSeriesService
{
    Task<SearchTvSeriesResponse> SearchTvSeriesAsync(string query, int page = 1,
        CancellationToken cancellationToken = default);

    Task<TvSeries?> GetTvSeriesAsync(int id, CancellationToken cancellationToken = default);

    Task AddToWatchlist(Guid userId, int seriesId);

    Task RemoveFromWatchlist(Guid userId, int seriesId);

    Task<List<TvSeries>> GetWatchlistAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddToCompleted(Guid userId, int seriesId);

    Task RemoveFromCompleted(Guid userId, int seriesId);

    Task<List<TvSeries>> GetCompletedAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddToWatching(Guid userId, int seriesId);

    Task StopWatching(Guid userId, int seriesId);

    Task ResumeWatching(Guid userId, int seriesId);

    Task RemoveFromWatching(Guid userId, int seriesId);

    Task<List<TvSeries>> GetWatchingAsync(Guid userId, CancellationToken cancellationToken = default);
}