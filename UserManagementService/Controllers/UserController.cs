using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Application.Dtos;
using UserManagementService.Application.Repositories.UserRepository;
using UserManagementService.Database.Models;

namespace UserManagementService.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _contextAccessor;

    public UserController(IUserRepository userRepository, IHttpContextAccessor contextAccessor)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(contextAccessor);

        _userRepository = userRepository;
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public async Task<IActionResult> GetUser()
    {
        if (Guid.TryParse(_contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) is false)
        {
            return BadRequest();
        }

        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto? user)
    {
        if (Guid.TryParse(_contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) is false)
        {
            return BadRequest();
        }

        if (user is null)
        {
            return BadRequest("User cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var id = await _userRepository.AddUserAsync(userId, user);
        return Ok(id);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto? user)
    {
        if (Guid.TryParse(_contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) is false)
        {
            return BadRequest();
        }

        if (user is null)
        {
            return BadRequest("User cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _userRepository.UpdateUserAsync(userId, user);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser()
    {
        if (Guid.TryParse(_contextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId) is false)
        {
            return BadRequest();
        }

        if (await _userRepository.DeleteUserAsync(userId))
        {
            return NoContent();
        }
        
        return NotFound("User not found.");
    }
}
