namespace PopcornBytes.Api.Episodes;

public class Episode
{
    public required Guid Id { get; init; }
    
    public required int ExternalId { get; init; }
    
    public required Guid TvSeriesId { get; init; }
    
    public int SeasonNumber { get; set; }
    
    public int EpisodeNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;

    public string EpisodeType { get; set; } = string.Empty;
    
    public int Runtime { get; set; }
    
    public DateOnly? ReleaseDate { get; set; }
}
