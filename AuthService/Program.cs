using AuthService.Application.Options;
using AuthService.Application.Services;
using AuthService.Application.Services.AuthManager;
using AuthService.Application.Services.JwtTokenManager;
using AuthService.Database;
using AuthService.Database.Models;
using AuthService.ExceptionHandlers;
using AuthService.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddAuthorizationBuilder();
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(x => x.GetRequiredService<IOptions<JwtOptions>>().Value);
builder.Services.AddSingleton<IJwtTokenManager, JwtTokenManager>();

builder.Services.AddIdentityCore<User>()
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AuthServiceDbContext>();

builder.Services.AddDbContext<AuthServiceDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration["DbConnectionString"]);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ArgumentExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAdminUserAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/register", async Task<Results<Ok, ValidationProblem>>
    ([FromBody] RegisterRequest registration, [FromServices] IAuthManager manager) =>
{
    var result = await manager.Register(registration.Email, registration.Password);
    if (!result.Succeeded)
    {
        return result.CreateValidationProblem();
    }

    return TypedResults.Ok();
});

app.MapPost("/login", async Task<Results<Ok<string>, EmptyHttpResult, ProblemHttpResult>>
    ([FromBody] LoginRequest login, [FromServices] IAuthManager manager) =>
{
    var result = await manager.Login(login.Email, login.Password);
    if (result is null)
    {
        return TypedResults.Problem("Invalid login or password", statusCode: StatusCodes.Status401Unauthorized);
    }

    return TypedResults.Ok(result);
});

app.Run();

public partial class Program;
