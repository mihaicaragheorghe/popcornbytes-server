namespace PopcornBytes.Api.Episodes;

public interface IEpisodeService
{
    Task<List<Episode>?> GetEpisodesAsync(int seriesId, int seasonNumber, CancellationToken cancellationToken = default);

    Task<Episode?> GetEpisodeAsync(int seriesId, int seasonNumber, int episodeNumber,
        CancellationToken cancellationToken = default);
}