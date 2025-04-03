using PopcornBytes.Api.Seasons;

namespace PopcornBytes.UnitTests.TestUtils;

public class SeasonTestUtils
{
    public static Season CreateSeason(
        int id = 1,
        int tvSeriesId = 1,
        string title = "Season 5",
        string overview =
            "Michael Scott and his fellow Dunder Mifflin-ites steal customers, frame co-workers, and indulge in intra-office love affairs.",
        int seasonNumber = 5,
        int episodeCount = 26,
        string? airDate = "2008-09-25",
        string? posterUrl = null) =>
        new()
        {
            Id = id,
            TvSeriesId = tvSeriesId,
            Title = title,
            Overview = overview,
            SeasonNumber = seasonNumber,
            EpisodeCount = episodeCount,
            AirDate = airDate is null ? null : Convert.ToDateTime(airDate),
            PosterUrl = posterUrl ?? $"/season-{seasonNumber}.jpg",
        };

    public static List<Season> CreateSeasons(int count) => Enumerable
        .Range(0, count)
        .Select(i => CreateSeason(id: i))
        .ToList();
}