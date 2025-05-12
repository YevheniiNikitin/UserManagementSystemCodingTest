using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Application.Dtos;
using UserManagementService.Application.Repositories.UserRepository;

namespace UserManagementService.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("[controller]")]
public class AdminUserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AdminUserController(IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);

        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto? user)
    {
        if (user is null)
        {
            return BadRequest("User cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var id = await _userRepository.AddUserAsync(user);
        return Ok(id);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] AdminUpdateUserDto? user)
    {
        if (user is null)
        {
            return BadRequest("User cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _userRepository.UpdateUserAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        if (await _userRepository.DeleteUserAsync(id))
        {
            return NoContent();
        }
        
        return NotFound("User not found.");
    }
}
