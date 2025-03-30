using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Series;

namespace PopcornBytes.UnitTests.TestUtils;

public static class TvSeriesTestUtils
{
    public static TvSeries CreateTvSeries(
        int id = 1,
        string name = "The Office",
        string overview = "I like Dwight",
        DateTime? lastAirDate = null,
        DateTime? firstAirDate = null,
        int seasonsCount = 9,
        int episodesCount = 186,
        bool inProduction = false,
        string status = "Ended",
        string tagline = "That's what she said",
        string posterUrl = "/dunder-mifflin-branch.jpg",
        Episode? lastEpisode = null,
        Episode? nextEpisode = null) =>
        new()
        {
            Id = id,
            Name = name,
            Overview = overview,
            LastAirDate = lastAirDate ?? new DateTime(2013, 05, 16),
            FirstAirDate = firstAirDate ?? new DateTime(2005, 03, 24),
            SeasonsCount = seasonsCount,
            EpisodesCount = episodesCount,
            InProduction = inProduction,
            Status = status,
            Tagline = tagline,
            PosterUrl = posterUrl,
            NextEpisode = nextEpisode,
            LastEpisode = lastEpisode ?? EpisodeTestUtils.CreateEpisode(seriesId: id)
        };
}