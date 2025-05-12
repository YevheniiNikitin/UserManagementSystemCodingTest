using System.Net.Http.Json;
using System.Net;
using FluentAssertions;

namespace AuthService.IntegrationTests;

public class MinimalApiTests : CustomWebApplicationFactory
{
    [Test]
    public async Task Register_ShouldReturnOk_WhenValid()
    {
        // Arrange
        var payload = new
        {
            Email = "testuser@example.com",
            Password = "StrongPassword123!"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenEmailIsNullOrEmptyString()
    {
        // Arrange
        var payload = new
        {
            Email = "",
            Password = "123"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/register", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsNullOrEmptyString()
    {
        var payload = new
        {
            Email = "valid@email.com",
            Password = ""
        };

        var response = await TestClient.PostAsJsonAsync("/register", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsNullOrEmptyString()
    {
        // Arrange
        var payload = new
        {
            Email = "",
            Password = "123"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsNullOrEmptyString()
    {
        var payload = new
        {
            Email = "valid@email.com",
            Password = ""
        };

        var response = await TestClient.PostAsJsonAsync("/login", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Login_ShouldReturnOkWithToken_WhenValidUserCredentials()
    {
        // Arrange
        var registerPayload = new
        {
            Email = "logintest@example.com", 
            Password = "StrongPassword123!"
        };
        await TestClient.PostAsJsonAsync("/register", registerPayload);

        // Act
        var loginPayload = new
        {
            Email = "logintest@example.com", 
            Password = "StrongPassword123!"
        };
        var response = await TestClient.PostAsJsonAsync("/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
    {
        // Arrange
        var payload = new
        {
            Email = "nonexistent@example.com", 
            Password = "WrongPassword!324"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/login", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenWrongPassword()
    {
        // Arrange
        var registerPayload = new
        {
            Email = "existinguser@example.com",
            Password = "StrongPassword123!"
        };
        await TestClient.PostAsJsonAsync("/register", registerPayload);

        // Act
        var loginPayload = new
        {
            Email = "existinguser@example.com",
            Password = "WrongPassword!324"
        };
        var response = await TestClient.PostAsJsonAsync("/login", loginPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
