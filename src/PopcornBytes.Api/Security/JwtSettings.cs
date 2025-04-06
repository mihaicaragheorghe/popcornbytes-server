using System.ComponentModel.DataAnnotations;

namespace PopcornBytes.Api.Security;

public class JwtSettings
{
    [Required]
    public string Audience { get; set; } = null!;
    
    [Required]
    public string Issuer { get; set; } = null!;
    
    [Required]
    public string Secret { get; set; } = null!;
    
    [Required]
    public int TokenExpirationInMinutes { get; set; }
}
