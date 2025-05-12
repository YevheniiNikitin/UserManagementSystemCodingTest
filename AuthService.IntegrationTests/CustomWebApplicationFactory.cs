using AuthService.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace AuthService.IntegrationTests;

[TestFixture]
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected readonly HttpClient TestClient;

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
            services.AddDbContext<AuthServiceDbContext>(options => { options.UseNpgsql(PostgresContainer.GetConnectionString()); });

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
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthServiceDbContext>();
            dbContext.Database.Migrate();

        });

        PostgresContainer.StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public new async Task DisposeAsync() => 
        await PostgresContainer.DisposeAsync();
}
