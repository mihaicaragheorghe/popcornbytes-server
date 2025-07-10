namespace PopcornBytes.Api.Series;

public interface ITvSeriesRepository
{
    Task<int> AddToWatchlistAsync(Guid userId, int seriesId, long addedAtUnix);

    Task<int> RemoveFromWatchlistAsync(Guid userId, int seriesId);

    Task<IEnumerable<int>> GetWatchlistAsync(Guid userId);

    Task<int> AddToCompletedAsync(Guid userId, int seriesId, long completedAtUnix);

    Task<int> RemoveFromCompletedAsync(Guid userId, int seriesId);

    Task<IEnumerable<int>> GetCompletedAsync(Guid userId);

    Task<int> AddToWatchingAsync(Guid userId, int seriesId, long startedAtUnix);

    Task<int> StopWatchingAsync(Guid userId, int seriesId, long stoppedAtUnix);

    Task<int> ResumeWatchingAsync(Guid userId, int seriesId, long resumedAtUnix);

    Task<int> RemoveFromWatchingAsync(Guid userId, int seriesId);

    Task<IEnumerable<int>> GetWatchingAsync(Guid userId);
}
