using PopcornBytes.Api.Tmdb.Contracts;

namespace PopcornBytes.Api.Episodes;

public class Episode
{
    public int Id { get; init; }
    
    public int SeriesId { get; init; }
    
    public int SeasonNumber { get; set; }
    
    public int EpisodeNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;

    public string EpisodeType { get; set; } = string.Empty;
    
    public int? Runtime { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    public string? StillUrl { get; set; }
    
    public static Episode FromTmdbEpisode(TmdbEpisode tmdbEpisode) =>
        new()
        {
            Id = tmdbEpisode.Id,
            SeriesId = tmdbEpisode.SeriesId,
            Title = tmdbEpisode.Name,
            Overview = tmdbEpisode.Overview ?? string.Empty,
            Runtime = tmdbEpisode.Runtime,
            SeasonNumber = tmdbEpisode.SeasonNumber,
            EpisodeNumber = tmdbEpisode.EpisodeNumber,
            EpisodeType = tmdbEpisode.EpisodeType ?? string.Empty,
            ReleaseDate = string.IsNullOrEmpty(tmdbEpisode.AirDate) ? null : Convert.ToDateTime(tmdbEpisode.AirDate),
            StillUrl = tmdbEpisode.StillPath
        };
}
