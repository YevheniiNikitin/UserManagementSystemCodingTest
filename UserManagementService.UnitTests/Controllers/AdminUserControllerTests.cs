using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using UserManagementService.Application.Dtos;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Controllers;
using UserManagementService.Database.Models;

namespace UserManagementService.UnitTests.Controllers;

public class AdminUserControllerTests
{
    private IUserRepository _userRepository = null!;
    private AdminUserController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _controller = new AdminUserController(_userRepository);
    }

    [Test]
    public async Task GetAllUsers_ShouldReturnOkWithUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test" },
            new() { Id = Guid.NewGuid(), Email = "test2@example.com", Name = "Test2" }
        };
        _userRepository.GetAllUsersAsync().Returns(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(users);
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "John", Email = "john@example.com" };
        _userRepository.GetUserByIdAsync(userId).Returns(user);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().BeEquivalentTo(user);
    }

    [Test]
    public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepository.GetUserByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        // Act
        var result = await _controller.GetUserById(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task AddUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Act
        var result = await _controller.AddUser(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.Value.Should().Be("User cannot be null.");
    }

    [Test]
    public async Task AddUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var user = new CreateUserDto();
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.AddUser(user);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task AddUser_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var user = new CreateUserDto { Name = "Jane", Email = "jane@example.com" };
        var id = Guid.NewGuid();
        _userRepository.AddUserAsync(user).Returns(id);

        // Act
        var result = await _controller.AddUser(user);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var createdResult = result as OkObjectResult;
        createdResult.Should().NotBeNull();
        createdResult.Value.Should().Be(id);
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Act
        var result = await _controller.UpdateUser(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.Value.Should().Be("User cannot be null.");
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("Email", "Required");
        var user = new AdminUpdateUserDto();

        // Act
        var result = await _controller.UpdateUser(user);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task UpdateUser_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var user = new AdminUpdateUserDto { Id = Guid.NewGuid(), Name = "Updated", Email = "updated@example.com" };

        // Act
        var result = await _controller.UpdateUser(user);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _userRepository.Received(1).UpdateUserAsync(user);
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNoContent_WhenUserDeleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.DeleteUserAsync(id).Returns(true);

        // Act
        var result = await _controller.DeleteUser(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _userRepository.DeleteUserAsync(id).Returns(false);

        // Act
        var result = await _controller.DeleteUser(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
              .Which.Value.Should().Be("User not found.");
    }
}
