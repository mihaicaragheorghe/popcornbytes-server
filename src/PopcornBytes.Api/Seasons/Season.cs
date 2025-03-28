using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.Seasons;

public class Season
{
    public required Guid Id { get; init; }
    
    public required int ExternalId { get; init; }
    
    public required Guid TvSeriesId { get; init; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Overview { get; set; } = string.Empty;
    
    public int SeasonNumber { get; set; }
    
    public DateOnly? AirDate { get; set; }
    
    public ICollection<Episode> Episodes { get; init; } = new List<Episode>();
}
