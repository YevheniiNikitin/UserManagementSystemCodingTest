using UserManagementService.Application.Dtos;
using UserManagementService.Database.Models;

namespace UserManagementService.Application.Repositories.UserRepository;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<IList<User>> GetAllUsersAsync();
    Task<Guid> AddUserAsync(CreateUserDto user);
    Task<Guid> AddUserAsync(Guid userId, CreateUserDto user);
    Task UpdateUserAsync(Guid userId, UpdateUserDto userDto);
    Task UpdateUserAsync(AdminUpdateUserDto user);
    Task<bool> DeleteUserAsync(Guid id);
}
