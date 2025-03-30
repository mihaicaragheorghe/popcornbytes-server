using System.Text.Json.Serialization;

using PopcornBytes.Api.Episodes;

namespace PopcornBytes.Api.Tmdb.Contracts;

public record TmdbEpisode
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("show_id")]
    public int SeriesId { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
    
    [JsonPropertyName("overview")]
    public string? Overview { get; init; }
    
    [JsonPropertyName("runtime")]
    public int? Runtime { get; init; }
    
    [JsonPropertyName("air_date")]
    public string? AirDate { get; init; }
    
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; init; }
    
    [JsonPropertyName("episode_number")]
    public int EpisodeNumber { get; init; }
    
    [JsonPropertyName("episode_type")]
    public string? EpisodeType { get; init; }
    
    [JsonPropertyName("still_path")]
    public string StillPath { get; set; } = string.Empty;

    public Episode ToEpisode() =>
        new()
        {
            Id = Id,
            SeriesId = SeriesId,
            Title = Name,
            Overview = Overview ?? string.Empty,
            Runtime = Runtime,
            SeasonNumber = SeasonNumber,
            EpisodeNumber = EpisodeNumber,
            EpisodeType = EpisodeType ?? string.Empty,
            ReleaseDate = AirDate is null ? null : Convert.ToDateTime(AirDate),
            StillUrl = StillPath
        };
}
