using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.TvSeries;

public class TvSeries
{
    public required Guid Id { get; init; }
    
    public required int ExternalId { get; init; }
    
    public required string Name { get; set; }

    public string Overview { get; set; } = string.Empty;

    public string PosterUrl { get; set; } = string.Empty;
    
    public int SeasonsCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public DateOnly? FirstAirDate { get; set; }
    
    public DateOnly? LastAirDate { get; set; }
    
    public bool InProduction { get; set; }

    public ICollection<string> Genres { get; set; } = new List<string>();
    
    public ICollection<string> Languages { get; set; } = new List<string>();

    public string Status { get; set; } = string.Empty;
    
    public Episode? NextEpisode { get; set; }
}
