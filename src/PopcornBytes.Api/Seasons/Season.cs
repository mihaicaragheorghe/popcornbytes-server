using PopcornBytes.Api.Tmdb.Contracts;

namespace PopcornBytes.Api.Seasons;

public class Season
{
    public int Id { get; init; }
    
    public int TvSeriesId { get; init; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;
    
    public int Number { get; set; }
    
    public int EpisodeCount { get; set; }
    
    public DateTime? AirDate { get; set; }
    
    public string? PosterUrl { get; set; }
    
    public static Season FromTmdbSeason(TmdbSeason tmdbSeason, int seriesId) =>
        new()
        {
            Id = tmdbSeason.Id,
            TvSeriesId = seriesId,
            Title = tmdbSeason.Name,
            Overview = tmdbSeason.Overview,
            Number = tmdbSeason.SeasonNumber,
            EpisodeCount = tmdbSeason.EpisodeCount,
            AirDate = string.IsNullOrEmpty(tmdbSeason.AirDate) ? null : Convert.ToDateTime(tmdbSeason.AirDate),
            PosterUrl = tmdbSeason.PosterPath
        };
}