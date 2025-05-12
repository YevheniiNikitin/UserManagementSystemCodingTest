using System.Net.Http.Json;
using System.Net;
using FluentAssertions;

namespace AuthService.IntegrationTests.Services;

public class IdentitySeederTests : CustomWebApplicationFactory
{
    [Test]
    public async Task LoginForAdmin_ShouldReturnOk_WhenValidAdminCredentials()
    {
        // Arrange
        var payloadForAdminUser = new
        {
            Email = "adminuser@no_email.com",
            Password = "!Q2W3e4r5t6y"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync("/login", payloadForAdminUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeNullOrEmpty();
    }
}
