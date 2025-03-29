namespace PopcornBytes.Api.Episodes;

public class Episode
{
    public int Id { get; init; }
    
    public int TvSeriesId { get; init; }
    
    public int SeasonNumber { get; set; }
    
    public int EpisodeNumber { get; set; }

    public string Title { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;

    public string EpisodeType { get; set; } = string.Empty;
    
    public int Runtime { get; set; }
    
    public DateOnly? ReleaseDate { get; set; }
}
