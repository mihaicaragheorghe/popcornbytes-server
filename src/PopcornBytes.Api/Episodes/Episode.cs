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
    
    public string StillUrl { get; set; } = string.Empty;
}
