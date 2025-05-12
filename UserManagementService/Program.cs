using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Database;
using UserManagementService.ExceptionHandlers;
using UserManagementService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddDbContext<UserManagementServiceDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration["DbConnectionString"]);
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<InvalidOperationExceptionHandler>();
builder.Services.AddExceptionHandler<KeyNotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        var publicKeyBytes = Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:JwtTokenIdentityProviderPublicKey"]!);
        var key = new SymmetricSecurityKey(publicKeyBytes);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtOptions:JwtTokenAudience"],
            ValidIssuer = builder.Configuration["JwtOptions:JwtTokenIssuer"],
            IssuerSigningKey = key
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SubClaimRequired", policy => policy.RequireClaim(JwtRegisteredClaimNames.Sub));
});

var app = builder.Build();
app.UseExceptionHandler();

app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
