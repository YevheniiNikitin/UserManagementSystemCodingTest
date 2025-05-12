using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Options;
public class JwtOptions
{
    public const string SectionName = "JwtOptions";

    [Required]
    public required string JwtTokenIssuer { get; set; }
    [Required]
    public required string JwtTokenAudience { get; set; }
    [Required]
    public required string JwtTokenIdentityProviderPublicKey { get; set; }

    public int JwtTokenExpirationInHours { get; set; } = 2;
    public int JwtTokenExpirationInMinutes { get; set; }
}
