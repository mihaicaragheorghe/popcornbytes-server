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
        TmdbEpisode? nextEpisode = null) =>
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
            LastEpisodeToAir = lastEpisode ?? CreateTmdbEpisode(seriesId: id)
        };

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
    
    public static SearchTvSeriesResult[] CreateSearchSeriesResultCollection(int count) => Enumerable
        .Range(1, count)
        .Select(i => CreateSearchSeriesResult(id: i))
        .ToArray();
}