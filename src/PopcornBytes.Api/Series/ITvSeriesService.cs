using PopcornBytes.Api.Kernel;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.Api.Series;

public interface ITvSeriesService
{
    Task<SearchTvSeriesResponse> QueryAsync(string query, int page = 1,
        CancellationToken cancellationToken = default);

    Task<TvSeries?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<List<TvSeries>> GetTrackedAsync(Guid userId, TrackedSeriesState state,
        CancellationToken cancellationToken = default);

    Task TrackAsync(Guid userId, int seriesId, TrackedSeriesState state);

    Task RemovedTrackedAsync(Guid userId, int seriesId, TrackedSeriesState state);

    Task<Result> StopWatchingAsync(Guid userId, int seriesId);

    Task<Result> ResumeWatchingAsync(Guid userId, int seriesId);
}