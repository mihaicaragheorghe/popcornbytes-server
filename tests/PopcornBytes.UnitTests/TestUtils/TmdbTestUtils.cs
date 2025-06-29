using PopcornBytes.Api.Tmdb.Contracts;
using PopcornBytes.Contracts.Series;

namespace PopcornBytes.UnitTests.TestUtils;

public static class TmdbTestUtils
{
    public static SearchTvSeriesResponse CreateSearchSeriesResponse(
        int pages = 1,
        int totalPages = 1,
        int totalResults = 1,
        SearchTvSeriesResult[]? results = null) =>
        new()
        {
            Page = pages,
            TotalPages = totalPages,
            TotalResults = totalResults,
            Results = results ?? CreateSearchSeriesResultCollection(totalResults)
        };

    public static SearchTvSeriesResult CreateSearchSeriesResult(
        int id = 1,
        string name = "Twin Peaks",
        string overview = "Laura Palmer",
        string posterPath = "/poster.jpg") =>
        new()
        {
            Id = id, Name = name, Overview = overview, PosterPath = posterPath,
        };

    public static TmdbTvSeries CreateTmdbTvSeries(
        int id = 1,
        string name = "The Office",
        string overview = "I like Dwight",
        string? lastAirDate = null,
        string? firstAirDate = null,
        int numberOfSeasons = 9,
        int numberOfEpisodes = 186,
        bool inProduction = false,
        string status = "Ended",
        string tagline = "That's what she said",
        string posterPath = "/dunder-mifflin-branch.jpg",
        TmdbEpisode? lastEpisode = null,
        TmdbEpisode? nextEpisode = null,
        List<TmdbSeason>? seasons = null) =>
        new()
        {
            Id = id,
            Name = name,
            Overview = overview,
            LastAirDate = lastAirDate ?? "2013-05-16",
            FirstAirDate = firstAirDate ?? "2005-03-24",
            NumberOfSeasons = numberOfSeasons,
            NumberOfEpisodes = numberOfEpisodes,
            InProduction = inProduction,
            Status = status,
            Tagline = tagline,
            PosterPath = posterPath,
            NextEpisodeToAir = nextEpisode,
            LastEpisodeToAir = lastEpisode ?? CreateTmdbEpisode(seriesId: id),
            Seasons = seasons ?? CreateTmdbSeasonCollection(9),
        };

    public static List<TmdbTvSeries> CreateTmdbTvSeriesCollection(int count, int startId = 1) =>
        Enumerable
            .Range(0, count)
            .Select(i => CreateTmdbTvSeries(id: startId + i))
            .ToList();

    public static TmdbEpisode CreateTmdbEpisode(
        int id = 1,
        int seriesId = 1,
        string name = "The Dinner Party",
        string overview = "Michael gets a plasma TV",
        int? runtime = null,
        string? airDate = null,
        int seasonNumber = 4,
        int episodeNumber = 13,
        string episodeType = "standard",
        string stillPath = "plasma-tv.jpg") =>
        new()
        {
            Id = id,
            Name = name,
            Overview = overview,
            AirDate = airDate ?? "2013-05-16",
            Runtime = runtime ?? 20,
            EpisodeNumber = episodeNumber,
            SeasonNumber = seasonNumber,
            EpisodeType = episodeType,
            SeriesId = seriesId,
            StillPath = stillPath,
        };

    public static TmdbSeason CreateTmdbSeason(
        int id = 1,
        string title = "Season 5",
        string overview =
            "Michael Scott and his fellow Dunder Mifflin-ites steal customers, frame co-workers, and indulge in intra-office love affairs.",
        int seasonNumber = 5,
        int episodeCount = 26,
        string? airDate = "2008-09-25",
        string posterUrl = "/hwlQxOPGIybqBz5TYYIe6XtOfi4.jpg",
        List<TmdbEpisode>? episodes = null) =>
        new()
        {
            Id = id,
            Name = title,
            Overview = overview,
            SeasonNumber = seasonNumber,
            EpisodeCount = episodeCount,
            AirDate = airDate,
            PosterPath = posterUrl,
            Episodes = episodes ?? CreateTmdbEpisodeCollection(13, id),
        };
    
    public static List<TmdbSeason> CreateTmdbSeasonCollection(int count, int startId = 1) => Enumerable
        .Range(0, count)
        .Select(i => CreateTmdbSeason(id: startId + i))
        .ToList();
    
    public static List<TmdbEpisode> CreateTmdbEpisodeCollection(int count, int seriesId = 1, int season = 1) => Enumerable
        .Range(1, count)
        .Select(i => CreateTmdbEpisode(id: i, episodeNumber: i, seriesId: seriesId, seasonNumber: season))
        .ToList();

    public static SearchTvSeriesResult[] CreateSearchSeriesResultCollection(int count, int startId = 1) => Enumerable
        .Range(0, count)
        .Select(i => CreateSearchSeriesResult(id: startId + i))
        .ToArray();
}