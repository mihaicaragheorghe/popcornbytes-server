using System.Text.Json.Serialization;

namespace PopcornBytes.Api.Tmdb.Contracts;

public record TmdbTvSeries
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
    
    [JsonPropertyName("overview")]
    public string Overview { get; init; }  = string.Empty;
    
    [JsonPropertyName("first_air_date")]
    public string? FirstAirDate { get; init; }
    
    [JsonPropertyName("last_air_date")]
    public string? LastAirDate { get; init; }
    
    [JsonPropertyName("number_of_seasons")]
    public int NumberOfSeasons { get; init; }
    
    [JsonPropertyName("number_of_episodes")]
    public int NumberOfEpisodes { get; init; }
    
    [JsonPropertyName("in_production")]
    public bool InProduction { get; init; }
    
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("tagline")]
    public string Tagline { get; init; } = string.Empty;
        
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("last_episode_to_air")]
    public TmdbEpisode? LastEpisodeToAir { get; init; }
    
    [JsonPropertyName("next_episode_to_air")]
    public TmdbEpisode? NextEpisodeToAir { get; init; }
    
    [JsonPropertyName("seasons")]
    public List<TmdbSeason> Seasons { get; init; } = [];
}
