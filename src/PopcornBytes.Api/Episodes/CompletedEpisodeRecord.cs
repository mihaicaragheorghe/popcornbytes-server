namespace PopcornBytes.Api.Episodes;

public record CompletedEpisodeRecord(
    Guid UserId,
    int SeriesId,
    int SeasonNumber,
    int EpisodeNumber,
    DateTime CompletedAt);
