using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Services.AuthManager;

public interface IAuthManager
{
    Task<IdentityResult> Register(string email, string password);

    Task<string?> Login(string email, string password);
}
