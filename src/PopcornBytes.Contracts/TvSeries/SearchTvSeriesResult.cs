using System.Text.Json.Serialization;

namespace PopcornBytes.Contracts.TvSeries;

public record SearchTvSeriesResult
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
    
    [JsonPropertyName("overview")]
    public string Overview { get; init; } = null!;
    
    [JsonPropertyName("poster_path")]
    public string PosterPath { get; init; } = null!;

    public string PosterUrl { get; set; } = string.Empty;
}