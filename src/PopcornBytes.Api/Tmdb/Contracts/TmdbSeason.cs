using System.Text.Json.Serialization;

namespace PopcornBytes.Api.Tmdb.Contracts;

public record TmdbSeason
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    
    [JsonPropertyName("overview")]
    public string Overview { get; init; } = string.Empty;
    
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; init; }
    
    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; init; }
    
    [JsonPropertyName("air_date")]
    public string? AirDate { get; init; }
    
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }
}
