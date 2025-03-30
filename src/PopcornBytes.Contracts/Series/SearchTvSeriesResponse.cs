using System.Text.Json.Serialization;

namespace PopcornBytes.Contracts.Series;

public record SearchTvSeriesResponse
{
    [JsonPropertyName("results")]
    public SearchTvSeriesResult[] Results { get; init; }  = [];
    
    [JsonPropertyName("page")]
    public int Page { get; init; }
    
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }
    
    [JsonPropertyName("total_results")]
    public int TotalResults { get; init; }
}
