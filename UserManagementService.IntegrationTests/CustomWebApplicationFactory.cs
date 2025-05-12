using System.IdentityModel.Tokens.Jwt;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Testcontainers.PostgreSql;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Database;
using Microsoft.Extensions.Options;
using System.Text;

namespace UserManagementService.IntegrationTests;

[TestFixture]
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected readonly HttpClient TestClient;
    protected IUserRepository UserRepository => 
        Services.CreateScope().ServiceProvider.GetRequiredService<IUserRepository>();

    protected static DateTime CurrentDateTime => DateTime.Parse("2/16/2000 12:15:00 PM");

    protected readonly PostgreSqlContainer PostgresContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .WithCleanUp(true)
        .Build();

    public CustomWebApplicationFactory()
    {
        TestClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<UserManagementServiceDbContext>(options => { options.UseNpgsql(PostgresContainer.GetConnectionString()); });

            services.RemoveAll(typeof(TimeProvider));
            services.AddSingleton(_ =>
            {
                var timeProvider = Substitute.For<TimeProvider>();
                timeProvider
                    .GetUtcNow()
                    .Returns(CurrentDateTime);
                return timeProvider;
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementServiceDbContext>();
            dbContext.Database.Migrate();
        });

        PostgresContainer.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    protected void AuthenticateAdminUserAsync()
    {
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            GetJwtAsync([
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")]));
    }

    protected Guid AuthenticateUserAsync()
    {
        var userId = Guid.NewGuid();
        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            GetJwtAsync([
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]));

        return userId;
    }

    private string GetJwtAsync(List<Claim> claims)
    {
        var publicKey = "UserManagementSystemPublicKey!@#@!wesrfaewfrasdfertgrafe";
        var publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);

        var key = new SymmetricSecurityKey(publicKeyBytes);
        var algorithm = SecurityAlgorithms.HmacSha256;
        var signingCredentials = new SigningCredentials(key, algorithm);

        var token = new JwtSecurityToken(
            "UserManagementSystemIdentityProvider",
            "UserManagementSystemAudience",
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public new async Task DisposeAsync() => 
        await PostgresContainer.DisposeAsync();
}
