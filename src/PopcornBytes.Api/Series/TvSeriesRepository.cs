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

    public async Task<int> AddToCompletedAsync(Guid userId, int seriesId, long completedAtUnix)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            INSERT OR IGNORE INTO series_completed (user_id, series_id, completed_at_unix)
            VALUES (@userId, @seriesId, @completedAtUnix);
            """,
            new { userId, seriesId, completedAtUnix });
    }

    public async Task<int> RemoveFromCompletedAsync(Guid userId, int seriesId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            "DELETE FROM series_completed WHERE user_id = @userId AND series_id = @seriesId;",
            new { userId, seriesId });
    }

    public async Task<IEnumerable<int>> GetCompletedAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.QueryAsync<int>(
            "SELECT series_id FROM series_completed WHERE user_id = @userId ORDER BY completed_at_unix DESC;",
            new { userId });
    }

    public async Task<int> AddToWatchingAsync(Guid userId, int seriesId, long addedAtUnix)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            INSERT OR IGNORE INTO series_watching (user_id, series_id, started_at_unix)
            VALUES (@userId, @seriesId, @addedAtUnix);
            """,
            new { userId, seriesId, addedAtUnix });
    }

    public async Task<int> StopWatchingAsync(Guid userId, int seriesId, long stoppedAtUnix)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            UPDATE series_watching
            SET is_stopped = 1, updated_at_unix = @stoppedAtUnix
            WHERE user_id = @userId AND series_id = @seriesId;
            """,
            new { userId, seriesId, stoppedAtUnix });
    }

    public async Task<int> ResumeWatchingAsync(Guid userId, int seriesId, long resumedAtUnix)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            UPDATE series_watching
            SET is_stopped = 0, updated_at_unix = @resumedAtUnix
            WHERE user_id = @userId AND series_id = @seriesId;
            """,
            new { userId, seriesId, resumedAtUnix });
    }

    public async Task<int> RemoveFromWatchingAsync(Guid userId, int seriesId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            "DELETE FROM series_watching WHERE user_id = @userId AND series_id = @seriesId;",
            new { userId, seriesId });
    }

    public async Task<IEnumerable<int>> GetWatchingAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.QueryAsync<int>(
            "SELECT series_id FROM series_watching WHERE user_id = @userId ORDER BY started_at_unix DESC;",
            new { userId });
    }

    public async Task<IEnumerable<int>> GetStoppedAsync(Guid userId)
    {
        using var connection = connectionFactory.CreateSqlConnection();

        return await connection.QueryAsync<int>(
            "SELECT series_id FROM series_watching WHERE user_id = @userId AND is_stopped = 1 ORDER BY updated_at_unix DESC;",
            new { userId });
    }
}