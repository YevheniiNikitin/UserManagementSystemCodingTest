using AuthService.Application.Services.JwtTokenManager;
using System.Security.Claims;
using AuthService.Application.Options;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Application.UnitTests.Services;

public class JwtTokenManagerTests
{
    [Test]
    public async Task GenerateToken_ShouldGenerateValidToken_WhenValidUser()
    {
        // Arrange
        var options = new JwtOptions
        {
            JwtTokenAudience = "JwtTokenAudience",
            JwtTokenIssuer = "JwtTokenIssuer",
            JwtTokenIdentityProviderPublicKey = "JwtTokenIdentityProviderPublicKey",
            JwtTokenExpirationInHours = 1,
            JwtTokenExpirationInMinutes = 7
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "Admin")
        };

        _timeProvider
            .GetUtcNow()
            .Returns(DateTime.Parse("2/16/2000 12:15:00 PM"));

        var sut = new JwtTokenManager(options, _timeProvider);

        // Act
        var token = sut.GenerateToken(claims);

        // Assert
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        await Verify(jwtToken);
    }


    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
}
