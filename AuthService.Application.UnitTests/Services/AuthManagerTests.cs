using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Application.Services.AuthManager;
using AuthService.Application.Services.JwtTokenManager;
using AuthService.Database.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AuthService.Application.UnitTests.Services;

public class AuthManagerTests
{
    private readonly AuthManager _sut;

    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenManager _jwtTokenManager;

    private const string ExpectedUsername = "username";
    private const string ExpectedPassword = "password";

    public AuthManagerTests()
    {
        _userManager = Substitute.For<UserManager<User>>(Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _jwtTokenManager = Substitute.For<IJwtTokenManager>();

        _sut = new AuthManager(NullLogger<AuthManager>.Instance, _userManager, _jwtTokenManager);
    }

    [Test]
    public async Task Register_ShouldCreateUser_WhenValidUser()
    {
        _userManager
            .CreateAsync(Arg.Any<User>(), Arg.Is(ExpectedPassword))
            .Returns(Task.FromResult(IdentityResult.Success));

        // Act
        var result = await _sut.Register(ExpectedUsername, ExpectedPassword);

        // Assert
        result.Succeeded.Should().BeTrue();
        await _userManager
            .Received(1)
            .CreateAsync(Arg.Any<User>(), Arg.Is(ExpectedPassword));
    }

    [Test]
    public async Task Login_ShouldReturnValidToken_WhenValidUser()
    {
        // Arrange
        var user = new User();

        _userManager
            .FindByEmailAsync(Arg.Is(ExpectedUsername))!
            .Returns(Task.FromResult(user));

        _userManager
            .CheckPasswordAsync(Arg.Is(user), Arg.Is(ExpectedPassword))
            .Returns(Task.FromResult(true));

        _jwtTokenManager
            .GenerateToken(Arg.Any<IEnumerable<Claim>>())
            .ReturnsForAnyArgs(CreateJwtToken([new Claim(ClaimTypes.Role, "Admin")]));

        // Act
        var response = await _sut.Login(ExpectedUsername, ExpectedPassword);

        // Assert
        response.Should().NotBeNullOrEmpty();

        await _userManager
            .Received(1)
            .FindByEmailAsync(Arg.Is(ExpectedUsername));

        await _userManager
            .Received(1)
            .CheckPasswordAsync(Arg.Is(user), Arg.Is(ExpectedPassword));

        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(response);
        await Verify(jwtToken);
    }

    [Test]
    public async Task Login_ShouldReturnNull_WhenUnableToFindUser()
    {
        // Arrange
        _userManager
            .FindByEmailAsync(Arg.Is(ExpectedUsername))
            .ReturnsNull();

        // Act
        var response = await _sut.Login(ExpectedUsername, ExpectedPassword);

        // Assert
        response.Should().BeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnBaseResponseFail_WhenWrongUsernameOrPassword()
    {
        // Arrange
        const string wrongPassword = "wrongPassword";
        var user = new User();

        _userManager
            .FindByEmailAsync(Arg.Is(ExpectedUsername))!
            .Returns(Task.FromResult(user));

        _userManager
            .CheckPasswordAsync(Arg.Is(user), Arg.Is(wrongPassword))
            .Returns(Task.FromResult(false));

        // Act
        var response = await _sut.Login(ExpectedUsername, wrongPassword);

        // Assert
        response.Should().BeNull();
    }

    [SetUp]
    protected void SetUp()
    {
        _userManager.ClearReceivedCalls();
    }

    [OneTimeTearDown]
    protected void TearDown()
    {
        _userManager.Dispose();
    }

    private static string CreateJwtToken(IEnumerable<Claim> claims)
    {
        var publicKeyBytes = "JwtTokenIdentityProviderPublicKey"u8.ToArray();
        var key = new SymmetricSecurityKey(publicKeyBytes);
        const string algorithm = SecurityAlgorithms.HmacSha256;
        var signingCredentials = new SigningCredentials(key, algorithm);

        var token = new JwtSecurityToken(
            "JwtTokenIssuer",
            "JwtTokenAudience",
            claims,
            notBefore: DateTime.Parse("2/16/2008 12:15:00 PM"),
            expires: DateTime.Parse("2/16/2008 12:15:00 PM").AddHours(1),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
