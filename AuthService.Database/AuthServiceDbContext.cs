using AuthService.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Database;

public class AuthServiceDbContext : IdentityDbContext<User, Role, string>
{
    public AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options)
        : base(options)
    { }
}
