using Dapper;

using PopcornBytes.Api.Persistence;

namespace PopcornBytes.Api.Series;

public class TvSeriesRepository(IDbConnectionFactory connectionFactory) : ITvSeriesRepository
{
    public async Task<int> AddToWatchlistAsync(Guid userId, int seriesId, long addedAtUnix)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            INSERT OR IGNORE INTO series_watchlist (user_id, series_id, added_at_unix)
            VALUES (@userId, @seriesId, @addedAtUnix);
            """,
            new { userId, seriesId, addedAtUnix });
    }

    public async Task<int> RemoveFromWatchlistAsync(Guid userId, int seriesId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            "DELETE FROM series_watchlist WHERE user_id = @userId AND series_id = @seriesId;",
            new { userId, seriesId });
    }

    public async Task<IEnumerable<int>> GetWatchlistAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.QueryAsync<int>(
            "SELECT series_id FROM series_watchlist WHERE user_id = @userId ORDER BY added_at_unix DESC;",
            new { userId });
    }
}