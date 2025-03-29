using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.Seasons;

public class Season
{
    public int Id { get; init; }
    
    public int TvSeriesId { get; init; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;
    
    public int SeasonNumber { get; set; }
    
    public DateOnly? AirDate { get; set; }
    
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
