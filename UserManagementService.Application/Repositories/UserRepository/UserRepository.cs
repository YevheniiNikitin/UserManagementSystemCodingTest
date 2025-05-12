using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Dtos;
using UserManagementService.Database;
using UserManagementService.Database.Models;

namespace UserManagementService.Application.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly UserManagementServiceDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public UserRepository(UserManagementServiceDbContext dbContext, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<IList<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    public async Task<Guid> AddUserAsync(CreateUserDto userDto)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == userDto.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Name = userDto.Name,
            Email = userDto.Email,
            CreatedAt = _timeProvider.GetUtcNow().DateTime
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user.Id;
    }

    public async Task<Guid> AddUserAsync(Guid userId, CreateUserDto userDto)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId || u.Email == userDto.Email);

        if (existingUser is not null && existingUser.Email == userDto.Email)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        if (existingUser is not null && existingUser.Id == userId)
        {
            throw new InvalidOperationException("You already have a user created. You can update or delete your user.");
        }

        var user = new User
        {
            Id = userId,
            Name = userDto.Name!,
            Email = userDto.Email!,
            CreatedAt = _timeProvider.GetUtcNow().DateTime
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user.Id;
    }

    public async Task UpdateUserAsync(Guid userId, UpdateUserDto userDto)
    {
        var user = await GetUserByIdAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found, please create a user first.");
        }

        if (userDto.Email != user.Email)
        {
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser is not null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
        }

        user.Name = userDto.Name;
        user.Email = userDto.Email;
        user.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(AdminUpdateUserDto userDto)
    {
        var user = await GetUserByIdAsync(userDto.Id);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (userDto.Email != user.Email)
        {
            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser is not null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }
        }

        user.Name = userDto.Name;
        user.Email = userDto.Email;
        user.UpdatedAt = _timeProvider.GetUtcNow().DateTime;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await GetUserByIdAsync(id);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var result = await _dbContext.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        await _dbContext.SaveChangesAsync();

        return result > 0;
    }
}
