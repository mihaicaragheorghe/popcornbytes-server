using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.TvSeries;

public class TvSeries
{
    public int Id { get; init; }

    public string Name { get; set; } = null!;

    public string Overview { get; set; } = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;
    
    public int SeasonsCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public DateOnly? FirstAirDate { get; set; }
    
    public DateOnly? LastAirDate { get; set; }
    
    public bool InProduction { get; set; }

    public string Status { get; set; } = string.Empty;
    
    public Episode? NextEpisode { get; set; }
}
