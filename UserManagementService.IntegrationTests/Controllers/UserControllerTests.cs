using FluentAssertions;
using System.Net.Http.Json;
using System.Net;
using UserManagementService.Application.Dtos;
using UserManagementService.Database.Models;

namespace UserManagementService.IntegrationTests.Controllers;

public class UserControllerTests : CustomWebApplicationFactory
{
    [Test]
    public async Task AllEndpoints_ShouldReturnUnauthorized_WhenNoAuthHeader()
    {
        TestClient.DefaultRequestHeaders.Authorization = null;

        var get = await TestClient.GetAsync("/User");
        var post = await TestClient.PostAsync("/User", null);
        var put = await TestClient.PutAsync("/User", null);
        var delete = await TestClient.DeleteAsync("/User");

        get.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        post.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        put.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        delete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetUser_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var userId = AuthenticateUserAsync();
        var userDto = new CreateUserDto { Name = "Jane Doe", Email = "jane@example.com" };
        await UserRepository.AddUserAsync(userId, userDto);

        // Act
        var response = await TestClient.GetAsync("/User");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<User>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(userDto.Email);
        result.Name.Should().Be(userDto.Name);
    }

    [Test]
    public async Task AddUser_ShouldReturnOkAndCreateUser_WhenValid()
    {
        // Arrange
        var userId = AuthenticateUserAsync();
        var dto = new CreateUserDto { Name = "Alice", Email = "alice@example.com" };

        // Act
        var response = await TestClient.PostAsJsonAsync("/User", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var id = Guid.Parse((await response.Content.ReadAsStringAsync()).Trim('"'));

        var created = await UserRepository.GetUserByIdAsync(id);
        created.Should().NotBeNull();
        created.Name.Should().Be(dto.Name);
        created.Email.Should().Be(dto.Email);
        created.Id.Should().Be(userId); // should match authenticated user id
    }

    [Test]
    public async Task AddUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Arrange
        AuthenticateUserAsync();

        // Act
        var response = await TestClient.PostAsync("/User", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task UpdateUser_ShouldUpdateAndReturnNoContent_WhenValid()
    {
        // Arrange
        var userId = AuthenticateUserAsync();
        await UserRepository.AddUserAsync(userId, new CreateUserDto
        {
            Name = "Initial Name",
            Email = "initial@example.com"
        });

        var dto = new UpdateUserDto { Name = "Updated", Email = "updated@example.com" };

        // Act
        var response = await TestClient.PutAsJsonAsync("/User", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updated = await UserRepository.GetUserByIdAsync(userId);
        updated!.Name.Should().Be(dto.Name);
        updated.Email.Should().Be(dto.Email);
    }

    [Test]
    public async Task UpdateUser_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Arrange
        AuthenticateUserAsync();

        // Act
        var response = await TestClient.PutAsync("/User", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNoContent_WhenUserExists()
    {
        // Arrange
        var userId = AuthenticateUserAsync();
        await UserRepository.AddUserAsync(userId, new CreateUserDto
        {
            Name = "ToDelete",
            Email = "delete@example.com"
        });

        // Act
        var response = await TestClient.DeleteAsync("/User");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var user = await UserRepository.GetUserByIdAsync(userId);
        user.Should().BeNull();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        AuthenticateUserAsync();

        // Act
        var response = await TestClient.DeleteAsync("/User");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}