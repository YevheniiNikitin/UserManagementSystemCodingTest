using System.Security.Claims;

namespace AuthService.Application.Services.JwtTokenManager;

public interface IJwtTokenManager
{
    public string GenerateToken(IEnumerable<Claim> claims);
}
