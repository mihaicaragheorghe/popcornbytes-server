using System.ComponentModel.DataAnnotations;

namespace PopcornBytes.Api.Tmdb;

public class TmdbOptions
{
    [Required]
    public string BaseURl { get; set; } = null!;
    
    [Required]
    public string ApiKey { get; set; } = null!;
    
    [Required]
    public string ImagesBaseUrl { get; set; } = null!;
}
