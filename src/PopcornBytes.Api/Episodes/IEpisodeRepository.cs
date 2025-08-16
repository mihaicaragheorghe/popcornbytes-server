namespace PopcornBytes.Api.Episodes;

public interface IEpisodeRepository
{
    Task<IEnumerable<CompletedEpisodeRecord>> GetCompletedAsync(Guid userId);

    Task<int> AddToCompletedAsync(Guid userId, int seriesId, int season, int episode, long watchedAtUnix);

    Task<int> RemoveFromCompletedAsync(Guid userId, int seriesId, int season, int episode);
}