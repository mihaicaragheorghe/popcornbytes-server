using PopcornBytes.Api.Episodes;
using PopcornBytes.Api.Seasons;
using PopcornBytes.Api.Tmdb.Contracts;

namespace PopcornBytes.Api.Series;

public class TvSeries
{
    public int Id { get; init; }

    public string Name { get; set; } = null!;

    public string Overview { get; set; } = string.Empty;
    
    public string Tagline { get; set; }  = string.Empty;

    public string? PosterUrl { get; set; }
    
    public int SeasonsCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public DateTime? FirstAirDate { get; set; }
    
    public DateTime? LastAirDate { get; set; }
    
    public bool InProduction { get; set; }

    public string Status { get; set; } = string.Empty;

    public Episode? LastEpisode { get; set; }
    
    public Episode? NextEpisode { get; set; }
    
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    
    public static TvSeries FromTmdbSeries(TmdbTvSeries tmdbSeries) =>
        new()
        {
            Id = tmdbSeries.Id,
            Name = tmdbSeries.Name,
            Overview = tmdbSeries.Overview,
            Tagline = tmdbSeries.Tagline,
            PosterUrl = tmdbSeries.PosterPath,
            SeasonsCount = tmdbSeries.NumberOfSeasons,
            EpisodesCount = tmdbSeries.NumberOfEpisodes,
            FirstAirDate = string.IsNullOrEmpty(tmdbSeries.FirstAirDate) ? null : Convert.ToDateTime(tmdbSeries.FirstAirDate),
            LastAirDate = string.IsNullOrEmpty(tmdbSeries.LastAirDate) ? null : Convert.ToDateTime(tmdbSeries.LastAirDate),
            InProduction = tmdbSeries.InProduction,
            Status = tmdbSeries.Status,
            LastEpisode = tmdbSeries.LastEpisodeToAir is null ? null : Episode.FromTmdbEpisode(tmdbSeries.LastEpisodeToAir),
            NextEpisode = tmdbSeries.NextEpisodeToAir is null ? null : Episode.FromTmdbEpisode(tmdbSeries.NextEpisodeToAir),
            Seasons = tmdbSeries.Seasons.Select(s => Season.FromTmdbSeason(s, tmdbSeries.Id)).ToList(),
        };
}
