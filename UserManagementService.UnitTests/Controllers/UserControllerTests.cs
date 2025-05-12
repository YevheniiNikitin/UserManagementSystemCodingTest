using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using FluentAssertions;
using UserManagementService.Application.Dtos;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Controllers;
using UserManagementService.Database.Models;

namespace UserManagementService.UnitTests.Controllers;

public class UserControllerTests
{
    private IUserRepository _userRepository = null!;
    private IHttpContextAccessor _httpContextAccessor = null!;
    private UserController _sut = null!;
    private Guid _userId;

    [SetUp]
    public void SetUp()
    {
        _userId = Guid.NewGuid();
        _userRepository = Substitute.For<IUserRepository>();

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, _userId.ToString())
        ]));

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _sut = new UserController(_userRepository, _httpContextAccessor);
    }

    [Test]
    public async Task GetUser_ShouldReturnBadRequest_WhenSubClaimIsMissing()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()) // no claims
        };

        _httpContextAccessor.HttpContext.Returns(httpContext);

        var controller = new UserController(_userRepository, _httpContextAccessor);

        // Act
        var result = await controller.GetUser();

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task GetUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = _userId, 
            Email = "test@example.com", 
            Name = "Test User"
        };
        _userRepository.GetUserByIdAsync(_userId).Returns(user);

        // Act
        var result = await _sut.GetUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(user);
    }

    [Test]
    public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepository.GetUserByIdAsync(_userId).Returns((User?)null);

        // Act
        var result = await _sut.GetUser();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task AddUser_ShouldReturnOk_WhenUserIsValid()
    {
        // Arrange
        var dto = new CreateUserDto { Email = "user@example.com", Name = "User" };
        var createdId = Guid.NewGuid();
        _userRepository.AddUserAsync(_userId, dto).Returns(createdId);

        // Act
        var result = await _sut.AddUser(dto);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(createdId);
    }

    [Test]
    public async Task AddUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Act
        var result = await _sut.AddUser(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("User cannot be null.");
    }

    [Test]
    public async Task AddUser_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var dto = new CreateUserDto();
        _sut.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _sut.AddUser(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateUser_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var dto = new UpdateUserDto { Email = "new@example.com", Name = "New Name" };

        // Act
        var result = await _sut.UpdateUser(dto);

        // Assert
        await _userRepository.Received(1).UpdateUserAsync(_userId, dto);
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Act
        var result = await _sut.UpdateUser(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("User cannot be null.");
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var dto = new UpdateUserDto();
        _sut.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _sut.UpdateUser(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNoContent_WhenDeleted()
    {
        // Arrange
        _userRepository.DeleteUserAsync(_userId).Returns(true);

        // Act
        var result = await _sut.DeleteUser();

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        _userRepository.DeleteUserAsync(_userId).Returns(false);

        // Act
        var result = await _sut.DeleteUser();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("User not found.");
    }
}
