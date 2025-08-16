using Dapper;
using PopcornBytes.Api.Persistence;

namespace PopcornBytes.Api.Episodes;

public class EpisodeRepository(IDbConnectionFactory dbConnectionFactory) : IEpisodeRepository
{
    public async Task<IEnumerable<CompletedEpisodeRecord>> GetCompletedAsync(Guid userId)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.QueryAsync<CompletedEpisodeRecord>(
            """
            SELECT
                user_id AS UserId,
                series_id AS SeriesId,
                season_number AS SeasonNumber,
                episode_number AS EpisodeNumber,
                watched_at_unix AS WatchedAtUnix
            FROM episodes_watched
            WHERE user_id = @userId
            """,
            new { userId });
    }

    public async Task<int> AddToCompletedAsync(Guid userId, int seriesId, int season, int episode, long watchedAtUnix)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            INSERT INTO episodes_watched (user_id, series_id, season_number, episode_number, watched_at_unix)
            VALUES (@userId, @seriesId, @season, @episode, @watchedAtUnix)
            """,
            new { userId, seriesId, season, episode, watchedAtUnix });
    }

    public async Task<int> RemoveFromCompletedAsync(Guid userId, int seriesId, int season, int episode)
    {
        using var connection = dbConnectionFactory.CreateSqlConnection();

        return await connection.ExecuteAsync(
            """
            DELETE FROM episodes_watch
            WHERE user_id = @userId, series_id = @seriesId, season_number = @season, episode_number = @episode
            """,
            new { userId, seriesId, season, episode });

    }
}