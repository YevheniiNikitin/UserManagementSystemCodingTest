using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Services.JwtTokenManager;

public class JwtTokenManager : IJwtTokenManager
{
    private readonly TimeProvider _timeProvider;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SigningCredentials _signingCredentials;
    private readonly TimeSpan _tokenExpirationTime;

    public JwtTokenManager(JwtOptions options, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _tokenExpirationTime =
            TimeSpan.FromHours(options.JwtTokenExpirationInHours)
                .Add(TimeSpan.FromMinutes(options.JwtTokenExpirationInMinutes));

        _issuer = options.JwtTokenIssuer;
        _audience = options.JwtTokenAudience;

        var publicKey = options.JwtTokenIdentityProviderPublicKey;
        var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);

        var key = new SymmetricSecurityKey(publicKeyBytes);
        var algorithm = SecurityAlgorithms.HmacSha256;
        _signingCredentials = new SigningCredentials(key, algorithm);
    }

    public string GenerateToken(IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            notBefore: _timeProvider.GetUtcNow().DateTime,
            expires: _timeProvider.GetUtcNow().DateTime.Add(_tokenExpirationTime),
            _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}