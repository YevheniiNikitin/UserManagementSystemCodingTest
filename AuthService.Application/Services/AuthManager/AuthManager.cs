using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Services.JwtTokenManager;
using AuthService.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services.AuthManager;

public class AuthManager : IAuthManager
{
    private readonly ILogger<AuthManager> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenManager _jwtTokenManager;

    public AuthManager(ILogger<AuthManager> logger, UserManager<User> userManager, IJwtTokenManager jwtTokenManager)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(jwtTokenManager);

        _logger = logger;
        _userManager = userManager;
        _jwtTokenManager = jwtTokenManager;
    }

    public async Task<IdentityResult> Register(string email, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var user = new User();
        await _userManager.SetUserNameAsync(user, email);
        await _userManager.SetEmailAsync(user, email);

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded is false)
        {
            return result;
        }

        await _userManager.AddClaimAsync(user, new(JwtRegisteredClaimNames.Sub, user.Id));
        _logger.LogInformation("{Email} user got registered", email);
        return result;
    }

    public async Task<string?> Login(string email, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        if (await _userManager.CheckPasswordAsync(user, password) is false)
        {
            return null;
        }

        var authClaims = await GetAuthClaims(user);
        return _jwtTokenManager.GenerateToken(authClaims);
    }

    private async Task<List<Claim>> GetAuthClaims(User user)
    {
        var authClaims = new List<Claim>();

        var userClaims = await _userManager.GetClaimsAsync(user);
        authClaims.AddRange(userClaims);

        var userRoles = await _userManager.GetRolesAsync(user);
        authClaims.AddRange(userRoles
            .Select(userRole => new Claim(ClaimTypes.Role, userRole)));

        return authClaims;
    }
}
