using PopcornBytes.Api.Episodes;

namespace PopcornBytes.UnitTests.TestUtils;

public static class EpisodeTestUtils
{
    public static Episode CreateEpisode(
        int id = 1,
        int seriesId = 1,
        string title = "The Dinner Party",
        string overview = "Michael gets a plasma TV",
        int? runtime = null,
        DateTime? releaseDate = null,
        int seasonNumber = 4,
        int episodeNumber = 13,
        string episodeType = "standard",
        string stillUrl = "plasma-tv.jpg") =>
        new()
        {
            Id = id,
            Title = title,
            Overview = overview,
            ReleaseDate = releaseDate ?? new DateTime(2013, 05, 16),
            Runtime = runtime ?? 20,
            EpisodeNumber = episodeNumber,
            SeasonNumber = seasonNumber,
            EpisodeType = episodeType,
            SeriesId = seriesId,
            StillUrl = stillUrl,
        };
    
    public static Episode[] CreateSearchSeriesResultCollection(int count) => Enumerable
        .Range(1, count)
        .Select(i => CreateEpisode(id: i))
        .ToArray();
}
