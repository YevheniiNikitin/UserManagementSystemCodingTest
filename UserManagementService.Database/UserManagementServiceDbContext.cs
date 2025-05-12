using Microsoft.EntityFrameworkCore;
using UserManagementService.Database.Models;

namespace UserManagementService.Database;

public class UserManagementServiceDbContext : DbContext
{
    static UserManagementServiceDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public UserManagementServiceDbContext(DbContextOptions<UserManagementServiceDbContext> options)
        : base(options)
    {

    }

    public DbSet<User> Users { get; set; }
}