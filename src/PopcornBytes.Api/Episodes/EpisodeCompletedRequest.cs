namespace PopcornBytes.Api.Episodes;

public record EpisodeCompletedRequest(int SeriesId, int SeasonNumber, int EpisodeNumber);