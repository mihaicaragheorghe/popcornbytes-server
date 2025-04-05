using System.Text.Json.Serialization;

namespace PopcornBytes.Api.Tmdb.Contracts;

public record TmdbConfiguration
{
    [JsonPropertyName("change_keys")]
    public string[] ChangeKeys { get; init; } = [];
    
    [JsonPropertyName("images")]
    public TmdbImageConfiguration Image { get; init; } = null!;
}

public record TmdbImageConfiguration
{
    [JsonPropertyName("secure_base_url")]
    public string BaseUrl { get; init; } = null!;
    
    [JsonPropertyName("backdrop_sizes")]
    public string[] BackdropSizes { get; init; } = null!;
    
    [JsonPropertyName("logo_sizes")]
    public string[] LogoSizes { get; init; } = null!;
    
    [JsonPropertyName("poster_sizes")]
    public string[] PosterSizes { get; init; } = null!;
    
    [JsonPropertyName("still_sizes")]
    public string[] StillSizes { get; init; } = null!;
}