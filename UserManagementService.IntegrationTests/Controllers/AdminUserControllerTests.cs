using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagementService.Application.Dtos;
using UserManagementService.Database;
using UserManagementService.Database.Models;

namespace UserManagementService.IntegrationTests.Controllers;

public class AdminUserControllerTests : CustomWebApplicationFactory
{
    [Test]
    public async Task AllEndpoints_ShouldReturnUnauthorized_WhenNoAuthHeader()
    {
        TestClient.DefaultRequestHeaders.Authorization = null;

        var getById = await TestClient.GetAsync("/AdminUser/4859e392-4fa2-4ed1-afbd-5dbe5b34cd12");
        var getAll = await TestClient.GetAsync("/AdminUser");
        var post = await TestClient.PostAsync("/User", null);
        var put = await TestClient.PutAsync("/User", null);
        var delete = await TestClient.DeleteAsync("/User");

        getById.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        getAll.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        post.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        put.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        delete.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllUsers_ShouldReturnOkWithUserList_WhenCalled()
    {
        // Arrange
        AuthenticateAdminUserAsync();
        var dbContext = Services.CreateScope().ServiceProvider.GetRequiredService<UserManagementServiceDbContext>();
        await dbContext.Users.ExecuteDeleteAsync();

        var user1 = new CreateUserDto { Email = "test@example.com", Name = "Test" };
        var user2 = new CreateUserDto { Email = "test2@example.com", Name = "Test2" };

        await UserRepository.AddUserAsync(user1);
        await UserRepository.AddUserAsync(user2);

        // Act
        var response = await TestClient.GetAsync("/AdminUser");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<User>>();
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().Contain(u => u.Email == user1.Email && u.Name == user1.Name);
        result.Should().Contain(u => u.Email == user2.Email && u.Name == user2.Name);
    }

    [Test]
    public async Task GetUserById_WhenUserExists_ShouldReturnOk()
    {
        // Arrange
        AuthenticateAdminUserAsync();
        var user = new CreateUserDto { Email = "test3@example.com", Name = "Test" };
        var id = await UserRepository.AddUserAsync(user);

        // Act
        var response = await TestClient.GetAsync($"/AdminUser/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<User>();
        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Name.Should().Be(user.Name);
    }

    [Test]
    public async Task AddUser_WhenValid_ShouldReturnCreated()
    {
        // Arrange
        AuthenticateAdminUserAsync();
        var user = new CreateUserDto { Name = "Charlie", Email = "charlie@example.com" };

        // Act
        var response = await TestClient.PostAsJsonAsync("/AdminUser", user);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var a = await response.Content.ReadAsStringAsync();

        var userFromDb = await UserRepository.GetUserByIdAsync(Guid.Parse((await response.Content.ReadAsStringAsync()).Trim('"')));
        userFromDb.Should().NotBeNull();
        userFromDb.Email.Should().Be(user.Email);
        userFromDb.Name.Should().Be(user.Name);
    }

    [Test]
    public async Task UpdateUser_WhenValid_ShouldReturnNoContent()
    {
        // Arrange
        AuthenticateAdminUserAsync();
        var user = new CreateUserDto { Email = "test4@example.com", Name = "Test" };
        var id = await UserRepository.AddUserAsync(user);

        var dto = new AdminUpdateUserDto
        {
            Id = id,
            Name = "UpdatedName",
            Email = "updated@example.com"
        };

        // Act
        var response = await TestClient.PutAsJsonAsync("/AdminUser", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var userFromDb = await UserRepository.GetUserByIdAsync(id);
        userFromDb.Should().NotBeNull();
        userFromDb.Email.Should().Be(dto.Email);
        userFromDb.Name.Should().Be(dto.Name);
    }

    [Test]
    public async Task DeleteUser_WhenUserExists_ShouldReturnNoContent()
    {
        // Arrange
        AuthenticateAdminUserAsync();
        var user = new CreateUserDto { Email = "test5@example.com", Name = "Test" };
        var id = await UserRepository.AddUserAsync(user);

        // Act
        var response = await TestClient.DeleteAsync($"/AdminUser/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var userFromDb = await UserRepository.GetUserByIdAsync(id);
        userFromDb.Should().BeNull();
    }
}
