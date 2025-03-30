using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.Series;

public class TvSeries
{
    public int Id { get; init; }

    public string Name { get; set; } = null!;

    public string Overview { get; set; } = string.Empty;
    
    public string Tagline { get; set; }  = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;
    
    public int SeasonsCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public DateTime? FirstAirDate { get; set; }
    
    public DateTime? LastAirDate { get; set; }
    
    public bool InProduction { get; set; }

    public string Status { get; set; } = string.Empty;

    public Episode? LastEpisode { get; set; }
    
    public Episode? NextEpisode { get; set; }
}
