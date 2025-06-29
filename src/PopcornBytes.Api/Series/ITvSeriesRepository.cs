namespace PopcornBytes.Api.Series;

public interface ITvSeriesRepository
{
    Task<int> AddToWatchlistAsync(Guid userId, int seriesId, long addedAtUnix);

    Task<int> RemoveFromWatchlistAsync(Guid userId, int seriesId);

    Task<IEnumerable<int>> GetWatchlistAsync(Guid userId);
}