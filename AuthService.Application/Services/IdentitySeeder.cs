using System.IdentityModel.Tokens.Jwt;
using AuthService.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Services;

public class IdentitySeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<IdentitySeeder>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

        const string roleName = "Admin";
        const string email = "adminuser@no_email.com";
        const string password = "!Q2W3e4r5t6y";

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            logger.LogInformation("Creating role {RoleName}", roleName);
            await roleManager.CreateAsync(new Role(roleName));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogInformation("Creating user {Email}", email);
            user = new User();
            await userManager.SetUserNameAsync(user, email);
            await userManager.SetEmailAsync(user, email);

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, roleName);
            await userManager.AddClaimAsync(user, new(JwtRegisteredClaimNames.Sub, user.Id));
        }
    }
}
